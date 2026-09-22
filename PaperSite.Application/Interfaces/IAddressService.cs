using PaperSite.Application.Common.Responses;
using PaperSite.Application.DTOs.Address;

namespace PaperSite.Application.Interfaces;

public interface IAddressService
{
    Task<BaseResponse<IEnumerable<AddressDto>>> GetMyAddressesAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<BaseResponse<AddressDto>> CreateAsync(Guid userId, CreateAddressRequest request, CancellationToken cancellationToken = default);

    Task<BaseResponse<AddressDto>> UpdateAsync(Guid userId, Guid addressId, UpdateAddressRequest request, CancellationToken cancellationToken = default);

    Task<BaseResponse<bool>> DeleteAsync(Guid userId, Guid addressId, CancellationToken cancellationToken = default);

    Task<BaseResponse<AddressDto>> SetDefaultAsync(Guid userId, Guid addressId, CancellationToken cancellationToken = default);
}