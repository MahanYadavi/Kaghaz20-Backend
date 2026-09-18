using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace PaperSite.API.Controllers;

[ApiController]
[Route("api-v1/[controller]/[action]")]
[Produces("application/json")]
public abstract class BaseController : ControllerBase
{
    protected Guid CurrentUserId
    {
        get
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(userId, out var id) ? id : Guid.Empty;
        }
    }
}
