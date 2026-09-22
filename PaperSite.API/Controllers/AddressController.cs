using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PaperSite.Application.DTOs.Address;
using PaperSite.Domain.Entities;
using PaperSite.Infrastructure.Persistence;

namespace PaperSite.API.Controllers;

[Authorize]
public class AddressController : BaseController
{
    private readonly ApplicationDbContext _context;

    public AddressController(ApplicationDbContext context)
    {
        _context = context;
    }


    /// <summary>
    /// دریافت آدرس‌های کاربر جاری
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetMyAddresses()
    {
        var addresses = await _context.UserAddresses
            .Where(x => x.UserId == CurrentUserId)
            .OrderByDescending(x => x.IsDefault)
            .ToListAsync();

        return Ok(addresses);
    }


    /// <summary>
    /// افزودن آدرس جدید
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create(createAddressDto request)
    {
        var hasAddress = await _context.UserAddresses
            .AnyAsync(x => x.UserId == CurrentUserId);


        var address = new UserAddress
        {
            Id = Guid.NewGuid(),
            UserId = CurrentUserId,

            Title = request.Title,
            Province = request.Province,
            City = request.City,
            FullAddress = request.FullAddress,
            PostalCode = request.PostalCode,

            // اولین آدرس به صورت پیش فرض انتخاب شود
            IsDefault = !hasAddress
        };


        _context.UserAddresses.Add(address);

        await _context.SaveChangesAsync();

        return Ok(address);
    }


    /// <summary>
    /// حذف آدرس
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var address = await _context.UserAddresses
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.UserId == CurrentUserId);


        if (address == null)
            return NotFound();


        _context.UserAddresses.Remove(address);

        await _context.SaveChangesAsync();

        return Ok();
    }


    /// <summary>
    /// انتخاب آدرس پیش فرض
    /// </summary>
    [HttpPut("{id:guid}/default")]
    public async Task<IActionResult> SetDefault(Guid id)
    {
        var addresses = await _context.UserAddresses
            .Where(x => x.UserId == CurrentUserId)
            .ToListAsync();


        foreach (var item in addresses)
        {
            item.IsDefault = item.Id == id;
        }


        await _context.SaveChangesAsync();

        return Ok();
    }
}