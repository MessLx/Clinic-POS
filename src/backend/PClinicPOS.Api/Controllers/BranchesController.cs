using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PClinicPOS.Api.Auth;
using PClinicPOS.Api.Data;
using PClinicPOS.Api.Models;

namespace PClinicPOS.Api.Controllers;

[Authorize(Policy = "Patients:View")]
public class BranchesController : ApiControllerBase
{
    private readonly AppDbContext _db;
    private readonly ITenantContext _tenant;

    public BranchesController(AppDbContext db, ITenantContext tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<BranchDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var tenantId = _tenant.TenantId;
        if (tenantId == null)
            return BadRequest(new ErrorResponse { Error = "TenantId is required." });

        var branches = await _db.Branches
            .AsNoTracking()
            .Where(b => b.TenantId == tenantId)
            .OrderBy(b => b.Name)
            .Select(b => new BranchDto(b.Id, b.TenantId, b.Name))
            .ToListAsync(ct);
        return Ok(branches);
    }
}

public record BranchDto(Guid Id, Guid TenantId, string Name);
