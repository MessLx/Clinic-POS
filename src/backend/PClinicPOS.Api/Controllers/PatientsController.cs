using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PClinicPOS.Api.Models;
using PClinicPOS.Api.Services;

namespace PClinicPOS.Api.Controllers;

public class PatientsController : ApiControllerBase
{
    private readonly IPatientService _patientService;
    private readonly Auth.ITenantContext _tenantContext;

    public PatientsController(IPatientService patientService, Auth.ITenantContext tenantContext)
    {
        _patientService = patientService;
        _tenantContext = tenantContext;
    }

    [HttpPost]
    [Authorize(Policy = "Patients:Create")]
    [ProducesResponseType(typeof(PatientResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreatePatientDto dto, CancellationToken ct)
    {
        var tenantId = _tenantContext.TenantId ?? dto.TenantId;
        if (tenantId == default)
            return BadRequest(new ErrorResponse { Error = "TenantId is required." });

        var req = new CreatePatientRequest(
            dto.FirstName?.Trim() ?? "",
            dto.LastName?.Trim() ?? "",
            dto.PhoneNumber?.Trim() ?? "",
            tenantId,
            dto.PrimaryBranchId);

        if (string.IsNullOrWhiteSpace(req.FirstName) || string.IsNullOrWhiteSpace(req.LastName) || string.IsNullOrWhiteSpace(req.PhoneNumber))
            return BadRequest(new ErrorResponse { Error = "FirstName, LastName, and PhoneNumber are required." });

        try
        {
            var patient = await _patientService.CreateAsync(req, ct);
            return CreatedAtAction(nameof(List), new { tenantId }, new PatientResponse(patient.Id, patient.FirstName, patient.LastName, patient.PhoneNumber, patient.TenantId, patient.PrimaryBranchId, patient.CreatedAt));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ErrorResponse { Error = ex.Message });
        }
    }

    [HttpGet]
    [Authorize(Policy = "Patients:View")]
    [ProducesResponseType(typeof(IReadOnlyList<PatientListItem>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] Guid tenantId, [FromQuery] Guid? branchId, CancellationToken ct)
    {
        var effectiveTenant = _tenantContext.TenantId ?? tenantId;
        if (effectiveTenant == default)
            return BadRequest(new ErrorResponse { Error = "TenantId is required." });

        try
        {
            var list = await _patientService.ListAsync(effectiveTenant, branchId, ct);
            return Ok(list);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }
}

public record CreatePatientDto(string FirstName, string LastName, string PhoneNumber, Guid TenantId, Guid? PrimaryBranchId);
public record PatientResponse(Guid Id, string FirstName, string LastName, string PhoneNumber, Guid TenantId, Guid? PrimaryBranchId, DateTime CreatedAt);
