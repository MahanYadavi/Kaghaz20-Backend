using PaperSite.Domain.Common;

namespace PaperSite.Domain.Entities;

public class Role : BaseEntity
{
    public const string Admin = "Admin";
    public const string Customer = "Customer";

    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ICollection<User> Users { get; set; } = new List<User>();
}
