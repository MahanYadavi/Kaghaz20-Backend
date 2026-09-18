using Microsoft.Extensions.Options;
using PaperSite.Application.Common.Responses;
using PaperSite.Application.Interfaces;
using System.Text;
using System.Text.Json;

namespace PaperSite.Infrastructure.Services
{
    public class SmsService : ISmsService
    {
        private readonly SmsSettings _settings;
        private readonly HttpClient _httpClient;

        public SmsService(
            IOptions<SmsSettings> options,
            HttpClient httpClient)
        {
            _settings = options.Value;
            _httpClient = httpClient;
        }

        public async Task<BaseResponse<bool>> SendOtpAsync(
            string mobile,
            string code)
        {
            mobile = NormalizeMobile(mobile);

            var request = new
            {
                mobile,
                templateId = int.Parse(_settings.TemplateId),
                parameters = new[]
                {
                    new
                    {
                        name = "Code",
                        value = code
                    }
                }
            };

            var json = JsonSerializer.Serialize(request);

            using var httpRequest = new HttpRequestMessage(
                HttpMethod.Post,
                "v1/send/verify"
            );

            httpRequest.Content =
                new StringContent(
                    json,
                    Encoding.UTF8,
                    "application/json"
                );

            httpRequest.Headers.Add(
                "x-api-key",
                _settings.ApiKey
            );

            using var response =
                await _httpClient.SendAsync(httpRequest);

            if (!response.IsSuccessStatusCode)
            {
                return BaseResponse<bool>.Failure(
                    "ارسال پیامک ناموفق بود"
                );
            }

            return BaseResponse<bool>.Success(
                true,
                "پیامک ارسال شد"
            );
        }

        public async Task<BaseResponse<bool>> SendPaymentSuccessAsync(
            string mobile,
            string buyerName,
            string trackingCode,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(mobile))
            {
                return BaseResponse<bool>.Failure(
                    "شماره موبایل خریدار موجود نیست"
                );
            }

            if (string.IsNullOrWhiteSpace(
                    _settings.PaymentSuccessTemplateId))
            {
                return BaseResponse<bool>.Failure(
                    "قالب پیامک پرداخت موفق تنظیم نشده است"
                );
            }

            mobile = NormalizeMobile(mobile);

            buyerName = string.IsNullOrWhiteSpace(buyerName)
                ? "خریدار"
                : buyerName.Trim();

            var request = new
            {
                mobile,

                templateId = int.Parse(
                    _settings.PaymentSuccessTemplateId
                ),

                parameters = new[]
                {
                    new
                    {
                        name = "Name",
                        value = buyerName
                    },
                    new
                    {
                        name = "TrackingCode",
                        value = trackingCode
                    }
                }
            };

            var json = JsonSerializer.Serialize(request);

            using var httpRequest = new HttpRequestMessage(
                HttpMethod.Post,
                "v1/send/verify"
            );

            httpRequest.Content =
                new StringContent(
                    json,
                    Encoding.UTF8,
                    "application/json"
                );

            httpRequest.Headers.Add(
                "x-api-key",
                _settings.ApiKey
            );

            try
            {
                using var response =
                    await _httpClient.SendAsync(
                        httpRequest,
                        cancellationToken
                    );

                var responseBody =
                    await response.Content.ReadAsStringAsync(
                        cancellationToken
                    );

                if (!response.IsSuccessStatusCode)
                {
                    return BaseResponse<bool>.Failure(
                        $"ارسال پیامک پرداخت موفق ناموفق بود. " +
                        $"StatusCode: {(int)response.StatusCode}"
                    );
                }

                return BaseResponse<bool>.Success(
                    true,
                    "پیامک پرداخت موفق ارسال شد"
                );
            }
            catch (Exception ex)
            {
                return BaseResponse<bool>.Failure(
                    $"خطا در ارسال پیامک: {ex.Message}"
                );
            }
        }

        private static string NormalizeMobile(string mobile)
        {
            mobile = mobile
                .Trim()
                .Replace(" ", "")
                .Replace("-", "");

            if (mobile.StartsWith("+98"))
            {
                return mobile[3..];
            }

            if (mobile.StartsWith("0098"))
            {
                return mobile[4..];
            }

            if (mobile.StartsWith("98"))
            {
                return mobile[2..];
            }

            if (mobile.StartsWith("0"))
            {
                return mobile[1..];
            }

            return mobile;
        }
    }
}