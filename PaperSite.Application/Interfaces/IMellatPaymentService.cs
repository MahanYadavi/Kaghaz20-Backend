using PaperSite.Application.Common.Responses;
using PaperSite.Application.DTOs.Payment;

namespace PaperSite.Application.Interfaces;

public interface IMellatPaymentService
{
    Task<BaseResponse<PaymentStartDto>> StartAsync(Guid userId, Guid orderId, CancellationToken cancellationToken = default);
    Task<string> ProcessCallbackAsync(MellatCallbackRequest request, CancellationToken cancellationToken = default);
}
