using PClinicPOS.Api.Models;

namespace PClinicPOS.Api.Services;

public interface IAppointmentService
{
    Task<Appointment> CreateAsync(CreateAppointmentRequest req, CancellationToken ct = default);
}

public record CreateAppointmentRequest(Guid TenantId, Guid BranchId, Guid PatientId, DateTime StartAt);
