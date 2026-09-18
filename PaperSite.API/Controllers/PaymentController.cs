using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaperSite.Application.Common.Responses;
using PaperSite.Application.DTOs.Payment;
using PaperSite.Application.Interfaces;

namespace PaperSite.API.Controllers;

public class PaymentController : BaseController
{
    private readonly IMellatPaymentService _paymentService;

    public PaymentController(IMellatPaymentService paymentService) => _paymentService = paymentService;

    [HttpPost]
    [Authorize(Policy = "CustomerPolicy")]
    [ProducesResponseType(typeof(BaseResponse<PaymentStartDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<PaymentStartDto>), StatusCodes.Status400BadRequest)]
    public new async Task<IActionResult> Request(PaymentRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _paymentService.StartAsync(CurrentUserId, request.OrderId, cancellationToken);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPost]
    [AllowAnonymous]
    [Consumes("application/x-www-form-urlencoded")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> MellatCallback(
        [FromForm] MellatCallbackRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var redirectUrl = await _paymentService.ProcessCallbackAsync(request, cancellationToken);
            return Redirect(redirectUrl);
        }
        catch (Exception ex)
        {
            // لاگ ex را در لاگ‌های سرور بررسی کن
            return Redirect(
                "https://www.kaghaz20.ir/payment/result?success=false&code=callback_error");
        }
    }
}
