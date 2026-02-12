using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PClinicPOS.Api.Models;
using PClinicPOS.Api.Services;

namespace PClinicPOS.Api.Controllers;

public class AppointmentsController : ApiControllerBase
{
    private readonly IAppointmentService _appointmentService;
    private readonly Auth.ITenantContext _tenantContext;

    public AppointmentsController(IAppointmentService appointmentService, Auth.ITenantContext tenantContext)
    {
        _appointmentService = appointmentService;
        _tenantContext = tenantContext;
    }

    [HttpPost]
    [Authorize(Policy = "Appointments:Create")]
    [ProducesResponseType(typeof(AppointmentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateAppointmentDto dto, CancellationToken ct)
    {
        var tenantId = _tenantContext.TenantId ?? dto.TenantId;
        if (tenantId == default || dto.BranchId == default || dto.PatientId == default)
            return BadRequest(new ErrorResponse { Error = "TenantId, BranchId, and PatientId are required." });

        var req = new CreateAppointmentRequest(tenantId, dto.BranchId, dto.PatientId, dto.StartAt);
        try
        {
            var appointment = await _appointmentService.CreateAsync(req, ct);
            return Created(string.Empty, new AppointmentResponse(appointment.Id, appointment.TenantId, appointment.BranchId, appointment.PatientId, appointment.StartAt, appointment.CreatedAt));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ErrorResponse { Error = ex.Message });
        }
    }
}

public record CreateAppointmentDto(Guid TenantId, Guid BranchId, Guid PatientId, DateTime StartAt);
public record AppointmentResponse(Guid Id, Guid TenantId, Guid BranchId, Guid PatientId, DateTime StartAt, DateTime CreatedAt);
