namespace PClinicPOS.Api.Models;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string Role { get; set; } = ""; // Admin, User, Viewer
    public Guid TenantId { get; set; }

    public Tenant? Tenant { get; set; }
    public ICollection<UserBranch> UserBranches { get; set; } = new List<UserBranch>();
}

public class UserBranch
{
    public Guid UserId { get; set; }
    public Guid BranchId { get; set; }

    public User? User { get; set; }
    public Branch? Branch { get; set; }
}
