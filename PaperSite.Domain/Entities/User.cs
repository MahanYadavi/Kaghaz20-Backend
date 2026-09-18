using PaperSite.Domain.Common;

namespace PaperSite.Domain.Entities;

public class User : BaseEntity
{
    public Guid RoleId { get; set; }
    public Role Role { get; set; } = null!;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public Cart? Cart { get; set; }
    public ICollection<Order> Orders { get; set; } = new List<Order>();

}
