using Microsoft.EntityFrameworkCore;
using PaperSite.Application.Common;
using PaperSite.Application.Common.Responses;
using PaperSite.Application.DTOs.Address;
using PaperSite.Application.Interfaces;
using PaperSite.Domain.Entities;
using PaperSite.Infrastructure.Persistence;

namespace PaperSite.Infrastructure.Services;

public class AddressService : IAddressService
{
    /// <summary>حداکثر تعداد آدرس هر کاربر.</summary>
    public const int MaxAddressesPerUser = 20;

    private readonly ApplicationDbContext _dbContext;

    public AddressService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<BaseResponse<IEnumerable<AddressDto>>> GetMyAddressesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // آدرس پیش‌فرض اول، سپس جدیدترین‌ها.
        var addresses = await _dbContext.UserAddresses
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.IsDefault)
            .ThenByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.Id)
            .ToListAsync(cancellationToken);

        return BaseResponse<IEnumerable<AddressDto>>.Success(addresses.Select(ToDto), "آدرس‌ها با موفقیت دریافت شدند");
    }

    public async Task<BaseResponse<AddressDto>> CreateAsync(Guid userId, CreateAddressRequest request, CancellationToken cancellationToken = default)
    {
        var titles = await _dbContext.UserAddresses
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .Select(x => new { x.Title, x.IsDefault })
            .ToListAsync(cancellationToken);

        if (titles.Count >= MaxAddressesPerUser)
        {
            return BaseResponse<AddressDto>.Failure($"حداکثر {MaxAddressesPerUser} آدرس برای هر کاربر مجاز است.");
        }

        var hasAddress = titles.Count > 0;

        var address = new UserAddress
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = request.Title.Trim(),
            Province = request.Province.Trim(),
            City = request.City.Trim(),
            FullAddress = request.FullAddress.Trim(),
            PostalCode = TextNormalizer.NormalizeDigitOnly(request.PostalCode) ?? string.Empty,

            // اولین آدرس کاربر به صورت پیش‌فرض انتخاب می‌شود.
            IsDefault = !hasAddress,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext.UserAddresses.Add(address);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return BaseResponse<AddressDto>.Success(ToDto(address), "آدرس با موفقیت ثبت شد");
    }

    public async Task<BaseResponse<AddressDto>> UpdateAsync(Guid userId, Guid addressId, UpdateAddressRequest request, CancellationToken cancellationToken = default)
    {
        var address = await FindOwnedAsync(userId, addressId, cancellationToken);
        if (address is null)
        {
            return BaseResponse<AddressDto>.Failure("آدرس انتخاب‌شده معتبر نیست.");
        }

        address.Title = request.Title.Trim();
        address.Province = request.Province.Trim();
        address.City = request.City.Trim();
        address.FullAddress = request.FullAddress.Trim();
        address.PostalCode = TextNormalizer.NormalizeDigitOnly(request.PostalCode) ?? string.Empty;
        address.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return BaseResponse<AddressDto>.Success(ToDto(address), "آدرس با موفقیت بروزرسانی شد");
    }

    public async Task<BaseResponse<bool>> DeleteAsync(Guid userId, Guid addressId, CancellationToken cancellationToken = default)
    {
        var address = await FindOwnedAsync(userId, addressId, cancellationToken);
        if (address is null)
        {
            return BaseResponse<bool>.Failure("آدرس انتخاب‌شده معتبر نیست.");
        }

        // حذف نرم: رفرنس AddressId در سفارش‌های قبلی سالم می‌ماند.
        address.IsDeleted = true;
        address.IsDefault = false;
        address.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);

        // اگر آدرس پیش‌فرض حذف شد، جدیدترین آدرس باقی‌مانده پیش‌فرض شود.
        var nextDefault = await _dbContext.UserAddresses
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (nextDefault is not null && !await _dbContext.UserAddresses.AnyAsync(x => x.UserId == userId && x.IsDefault, cancellationToken))
        {
            nextDefault.IsDefault = true;
            nextDefault.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return BaseResponse<bool>.Success(true, "آدرس با موفقیت حذف شد");
    }

    public async Task<BaseResponse<AddressDto>> SetDefaultAsync(Guid userId, Guid addressId, CancellationToken cancellationToken = default)
    {
        var addresses = await _dbContext.UserAddresses
            .Where(x => x.UserId == userId)
            .ToListAsync(cancellationToken);

        var target = addresses.FirstOrDefault(x => x.Id == addressId);
        if (target is null)
        {
            return BaseResponse<AddressDto>.Failure("آدرس انتخاب‌شده معتبر نیست.");
        }

        foreach (var address in addresses)
        {
            address.IsDefault = address.Id == addressId;
        }

        target.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return BaseResponse<AddressDto>.Success(ToDto(target), "آدرس پیش‌فرض با موفقیت تغییر یافت");
    }

    private Task<UserAddress?> FindOwnedAsync(Guid userId, Guid addressId, CancellationToken cancellationToken) =>
        _dbContext.UserAddresses.FirstOrDefaultAsync(x => x.Id == addressId && x.UserId == userId, cancellationToken);

    private static AddressDto ToDto(UserAddress address) => new()
    {
        Id = address.Id,
        Title = address.Title,
        Province = address.Province,
        City = address.City,
        FullAddress = address.FullAddress,
        PostalCode = address.PostalCode,
        IsDefault = address.IsDefault
    };
}