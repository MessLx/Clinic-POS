namespace PClinicPOS.Api.Models;

public class Patient
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string PhoneNumber { get; set; } = "";
    public Guid TenantId { get; set; }
    public Guid? PrimaryBranchId { get; set; }
    public DateTime CreatedAt { get; set; }

    public Tenant? Tenant { get; set; }
    public Branch? PrimaryBranch { get; set; }
}
