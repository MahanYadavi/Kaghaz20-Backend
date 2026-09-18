using PaperSite.Domain.Entities;

namespace PaperSite.Application.Interfaces;

public interface IJWtService
{
    string GenerateToken(User user);
    string GenerateRegistrationToken(string phoneNumber);

    string? ValidateRegistrationToken(string token);
}
