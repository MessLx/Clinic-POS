using PClinicPOS.Api.Models;

namespace PClinicPOS.Api.Services;

public interface IPatientService
{
    Task<Patient> CreateAsync(CreatePatientRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<PatientListItem>> ListAsync(Guid tenantId, Guid? branchId, CancellationToken ct = default);
}

public record CreatePatientRequest(string FirstName, string LastName, string PhoneNumber, Guid TenantId, Guid? PrimaryBranchId);

public record PatientListItem(Guid Id, string FirstName, string LastName, string PhoneNumber, Guid TenantId, Guid? PrimaryBranchId, DateTime CreatedAt);
