using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PaperSite.Application.Common.Responses;
using PaperSite.Application.Configuration;
using PaperSite.Application.DTOs.Payment;
using PaperSite.Application.Interfaces;
using PaperSite.Domain.Entities;
using PaperSite.Domain.Enums;
using PaperSite.Infrastructure.Persistence;

namespace PaperSite.Infrastructure.Services;

public class MellatPaymentService : IMellatPaymentService
{
    private static readonly HashSet<string> SuccessfulVerifyCodes = ["0", "43", "45"];
    private static readonly HashSet<string> SuccessfulSettleCodes = ["0", "45"];
    private readonly HttpClient _httpClient;
    private readonly ApplicationDbContext _dbContext;
    private readonly MellatPaymentSettings _settings;
    private readonly ILogger<MellatPaymentService> _logger;
    private readonly ISmsService _smsService;
    public MellatPaymentService(HttpClient httpClient, ApplicationDbContext dbContext,
        IOptions<MellatPaymentSettings> settings, ILogger<MellatPaymentService> logger,ISmsService smsService)
    {
        _httpClient = httpClient;
        _dbContext = dbContext;
        _settings = settings.Value;
        _logger = logger;
        _smsService = smsService;

    }

    public async Task<BaseResponse<PaymentStartDto>> StartAsync(Guid userId, Guid orderId,
        CancellationToken cancellationToken = default)
    {
        if (_settings.TerminalId <= 0 || string.IsNullOrWhiteSpace(_settings.UserName) ||
            string.IsNullOrWhiteSpace(_settings.UserPassword) || string.IsNullOrWhiteSpace(_settings.CallbackUrl))
            return BaseResponse<PaymentStartDto>.Failure("تنظیمات درگاه ملت روی سرور کامل نشده است");

        var order = await _dbContext.Orders
            .FirstOrDefaultAsync(x => x.Id == orderId && x.UserId == userId, cancellationToken);
        if (order is null)
            return BaseResponse<PaymentStartDto>.Failure("سفارش یافت نشد");
        if (order.Status == OrderStatus.Paid)
            return BaseResponse<PaymentStartDto>.Failure("این سفارش قبلاً پرداخت شده است");
        if (order.Status != OrderStatus.Pending)
            return BaseResponse<PaymentStartDto>.Failure("این سفارش در وضعیت قابل پرداخت نیست");

        var reusablePayment = await _dbContext.Payments
            .Where(x => x.OrderId == order.Id && x.Status == PaymentStatus.Redirected && x.RefId != null &&
                        x.CreatedAt >= DateTime.UtcNow.AddMinutes(-15))
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
        if (reusablePayment is not null)
        {
            return BaseResponse<PaymentStartDto>.Success(new PaymentStartDto
            {
                PaymentUrl = _settings.PaymentPageUrl,
                RefId = reusablePayment.RefId!
            });
        }

        long amountRial;
        try
        {
            amountRial = checked(decimal.ToInt64(order.TotalAmount * 10m));
        }
        catch (OverflowException)
        {
            return BaseResponse<PaymentStartDto>.Failure("مبلغ سفارش برای درگاه نامعتبر است");
        }
        if (amountRial <= 0)
            return BaseResponse<PaymentStartDto>.Failure("مبلغ سفارش باید بیشتر از صفر باشد");

        var now = DateTime.Now;
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            GatewayOrderId = CreateGatewayOrderId(),
            AmountToman = order.TotalAmount,
            AmountRial = amountRial,
            Status = PaymentStatus.Pending
        };
        _dbContext.Payments.Add(payment);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var response = await CallAsync("bpPayRequest", new Dictionary<string, string>
        {
            ["terminalId"] = _settings.TerminalId.ToString(CultureInfo.InvariantCulture),
            ["userName"] = _settings.UserName,
            ["userPassword"] = _settings.UserPassword,
            ["orderId"] = payment.GatewayOrderId.ToString(CultureInfo.InvariantCulture),
            ["amount"] = payment.AmountRial.ToString(CultureInfo.InvariantCulture),
            ["localDate"] = now.ToString("yyyyMMdd", CultureInfo.InvariantCulture),
            ["localTime"] = now.ToString("HHmmss", CultureInfo.InvariantCulture),
            ["additionalData"] = $"Order:{order.Id:N}",
            ["callBackUrl"] = _settings.CallbackUrl,
            ["payerId"] = "0"
        }, cancellationToken);

        var parts = response.Split(',', 2, StringSplitOptions.TrimEntries);
        payment.ResCode = parts[0];
        if (parts.Length != 2 || parts[0] != "0" || string.IsNullOrWhiteSpace(parts[1]))
        {
            payment.Status = PaymentStatus.Failed;
            payment.ErrorMessage = GetResponseMessage(parts[0]);
            payment.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);
            return BaseResponse<PaymentStartDto>.Failure($"خطای درگاه ملت: {payment.ErrorMessage}");
        }

        payment.RefId = parts[1];
        payment.Status = PaymentStatus.Redirected;
        payment.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return BaseResponse<PaymentStartDto>.Success(new PaymentStartDto
        {
            PaymentUrl = _settings.PaymentPageUrl,
            RefId = payment.RefId
        }, "درخواست پرداخت با موفقیت ایجاد شد");
    }

    public async Task<string> ProcessCallbackAsync(MellatCallbackRequest request,
        CancellationToken cancellationToken = default)
    {
        var payment = await _dbContext.Payments.Include(x => x.Order)
            .FirstOrDefaultAsync(x => x.GatewayOrderId == request.SaleOrderId, cancellationToken);

        if (payment is null || string.IsNullOrWhiteSpace(request.RefId) ||
            !string.Equals(payment.RefId, request.RefId, StringComparison.Ordinal))
            return BuildResultUrl(false, request.SaleOrderId, "invalid_callback");
        if (payment.Status == PaymentStatus.Succeeded || payment.Order.Status == OrderStatus.Paid)
            return BuildResultUrl(true, payment.GatewayOrderId, "already_paid");

        payment.ResCode = request.ResCode;
        payment.SaleReferenceId = request.SaleReferenceId;
        payment.UpdatedAt = DateTime.UtcNow;
        if (request.ResCode != "0" || request.SaleReferenceId <= 0)
        {
            payment.Status = PaymentStatus.Failed;
            payment.ErrorMessage = GetResponseMessage(request.ResCode);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return BuildResultUrl(false, payment.GatewayOrderId, request.ResCode ?? "invalid_response");
        }

        payment.Status = PaymentStatus.Verifying;
        await _dbContext.SaveChangesAsync(cancellationToken);
        var verifyCode = await CallTransactionAsync("bpVerifyRequest", payment, cancellationToken);
        if (!SuccessfulVerifyCodes.Contains(verifyCode))
        {
            payment.Status = PaymentStatus.Failed;
            payment.ResCode = verifyCode;
            payment.ErrorMessage = GetResponseMessage(verifyCode);
            payment.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);
            return BuildResultUrl(false, payment.GatewayOrderId, verifyCode);
        }

        var settleCode = verifyCode == "45" ? "45" :
            await CallTransactionAsync("bpSettleRequest", payment, cancellationToken);
        if (!SuccessfulSettleCodes.Contains(settleCode))
        {
            payment.Status = PaymentStatus.Failed;
            payment.ResCode = settleCode;
            payment.ErrorMessage = GetResponseMessage(settleCode);
            payment.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);
            return BuildResultUrl(false, payment.GatewayOrderId, settleCode);
        }
        payment.Status = PaymentStatus.Succeeded;
        payment.ResCode = "0";
        payment.ErrorMessage = null;
        payment.VerifiedAt = DateTime.UtcNow;
        payment.UpdatedAt = DateTime.UtcNow;

        payment.Order.Status = OrderStatus.Paid;
        payment.Order.UpdatedAt = DateTime.UtcNow;

        // اول پرداخت قطعی داخل دیتابیس ذخیره شود
        await _dbContext.SaveChangesAsync(cancellationToken);

        // بعد از پرداخت موفق، پیامک ارسال می‌شود
        try
        {
            var buyerName =
                string.IsNullOrWhiteSpace(
                    payment.Order.ReceiverFullName)
                    ? "خریدار"
                    : payment.Order.ReceiverFullName.Trim();

            var mobile =
                payment.Order.ReceiverPhoneNumber;

            var trackingCode =
                payment.GatewayOrderId.ToString(
                    CultureInfo.InvariantCulture);

            var smsResult =
                await _smsService.SendPaymentSuccessAsync(
                    mobile,
                    buyerName,
                    trackingCode,
                    cancellationToken
                );

            if (!smsResult.IsSuccess)
            {
                _logger.LogWarning(
                    "Payment succeeded but SMS failed. " +
                    "OrderId: {OrderId}, " +
                    "TrackingCode: {TrackingCode}, " +
                    "Message: {Message}",
                    payment.OrderId,
                    payment.GatewayOrderId,
                    smsResult.Message
                );
            }
            else
            {
                _logger.LogInformation(
                    "Payment success SMS sent. " +
                    "OrderId: {OrderId}, " +
                    "TrackingCode: {TrackingCode}",
                    payment.OrderId,
                    payment.GatewayOrderId
                );
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Payment succeeded but payment success SMS failed. " +
                "OrderId: {OrderId}, " +
                "TrackingCode: {TrackingCode}",
                payment.OrderId,
                payment.GatewayOrderId
            );
        }

        return BuildResultUrl(
            true,
            payment.GatewayOrderId,
            "0"
        );

    }

    private Task<string> CallTransactionAsync(string method, Payment payment, CancellationToken cancellationToken) =>
        CallAsync(method, new Dictionary<string, string>
        {
            ["terminalId"] = _settings.TerminalId.ToString(CultureInfo.InvariantCulture),
            ["userName"] = _settings.UserName,
            ["userPassword"] = _settings.UserPassword,
            ["orderId"] = payment.GatewayOrderId.ToString(CultureInfo.InvariantCulture),
            ["saleOrderId"] = payment.GatewayOrderId.ToString(CultureInfo.InvariantCulture),
            ["saleReferenceId"] = payment.SaleReferenceId!.Value.ToString(CultureInfo.InvariantCulture)
        }, cancellationToken);

    private async Task<string> CallAsync(string method, IReadOnlyDictionary<string, string> parameters,
        CancellationToken cancellationToken)
    {
        XNamespace soap = "http://schemas.xmlsoap.org/soap/envelope/";
        XNamespace pgw = "http://interfaces.core.sw.bps.com/";
        var methodElement = new XElement(pgw + method, parameters.Select(x => new XElement(x.Key, x.Value)));
        var document = new XDocument(new XElement(soap + "Envelope",
            new XAttribute(XNamespace.Xmlns + "soapenv", soap),
            new XAttribute(XNamespace.Xmlns + "pgw", pgw),
            new XElement(soap + "Header"), new XElement(soap + "Body", methodElement)));

        using var message = new HttpRequestMessage(HttpMethod.Post, _settings.WebServiceUrl);
        message.Content = new StringContent(document.ToString(SaveOptions.DisableFormatting), Encoding.UTF8, "text/xml");
        message.Headers.TryAddWithoutValidation("SOAPAction", "\"\"");
        message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/xml"));
        using var response = await _httpClient.SendAsync(message, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        response.EnsureSuccessStatusCode();

        var responseDocument = XDocument.Parse(responseBody);
        var fault = responseDocument.Descendants().FirstOrDefault(x => x.Name.LocalName == "faultstring")?.Value;
        if (!string.IsNullOrWhiteSpace(fault))
            throw new InvalidOperationException("درگاه ملت پاسخ نامعتبر برگرداند");
        var result = responseDocument.Descendants().FirstOrDefault(x => x.Name.LocalName == "return")?.Value;
        if (string.IsNullOrWhiteSpace(result))
            throw new InvalidOperationException("پاسخ درگاه ملت قابل خواندن نیست");
        _logger.LogInformation("Mellat method {Method} returned response code {ResponseCode}", method, result.Split(',')[0]);
        return result.Trim();
    }

    private string BuildResultUrl(bool success, long trackingCode, string code)
    {
        var separator = _settings.FrontendResultUrl.Contains('?') ? '&' : '?';
        return $"{_settings.FrontendResultUrl}{separator}success={success.ToString().ToLowerInvariant()}&trackingCode={trackingCode}&code={Uri.EscapeDataString(code)}";
    }

    private static long CreateGatewayOrderId() =>
        checked(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() * 1000 + Random.Shared.Next(100, 1000));

    private static string GetResponseMessage(string? code) => code switch
    {
        "0" => "عملیات موفق بود", "11" => "شماره کارت نامعتبر است", "12" => "موجودی کافی نیست",
        "13" => "رمز کارت نادرست است", "17" => "کاربر از انجام تراکنش منصرف شد",
        "21" => "پذیرنده نامعتبر است", "24" => "اطلاعات کاربری پذیرنده نامعتبر است",
        "25" => "مبلغ نامعتبر است", "34" => "خطای سیستمی درگاه", "41" => "شماره درخواست تکراری است",
        "43" => "تراکنش قبلاً تأیید شده است", "45" => "تراکنش قبلاً تسویه شده است",
        "48" => "تراکنش برگشت داده شده است", "62" => "آدرس بازگشت با دامنه ثبت‌شده تطابق ندارد",
        "421" => "IP سرور پذیرنده ثبت نشده است", null or "" => "پاسخ نامعتبر از درگاه", _ => $"کد پاسخ {code}"
    };
}
