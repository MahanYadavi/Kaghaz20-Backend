using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaperSite.Application.Common.Responses;

namespace PaperSite.API.Controllers;

public class TestController : BaseController
{
    /// <summary>
    /// تست احراز هویت و مجوز دسترسی
    /// </summary>
    /// <returns>
    /// صحت توکن JWT و دسترسی‌های کاربر جاری را بررسی می‌کند.
    /// </returns>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status200OK)]
    public IActionResult TestJwt()
    {
        return Ok(BaseResponse<string>.Success("authorized", "JWT token is valid"));
    }
}
