namespace PClinicPOS.Api.Models;

public class Branch
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Name { get; set; } = "";

    public Tenant? Tenant { get; set; }
}
