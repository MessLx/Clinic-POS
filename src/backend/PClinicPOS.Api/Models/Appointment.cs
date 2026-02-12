namespace PClinicPOS.Api.Models;

public class Appointment
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid BranchId { get; set; }
    public Guid PatientId { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime CreatedAt { get; set; }

    public Tenant? Tenant { get; set; }
    public Branch? Branch { get; set; }
    public Patient? Patient { get; set; }
}
