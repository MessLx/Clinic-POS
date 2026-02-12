using Microsoft.EntityFrameworkCore;
using Npgsql;
using PClinicPOS.Api.Data;
using PClinicPOS.Api.Auth;
using PClinicPOS.Api.Models;

namespace PClinicPOS.Api.Services;

public class PatientService : IPatientService
{
    private readonly AppDbContext _db;
    private readonly ITenantContext _tenant;
    private readonly ICacheService _cache;

    public PatientService(AppDbContext db, ITenantContext tenant, ICacheService cache)
    {
        _db = db;
        _tenant = tenant;
        _cache = cache;
    }

    public async Task<Patient> CreateAsync(CreatePatientRequest req, CancellationToken ct = default)
    {
        if (_tenant.TenantId != null && _tenant.TenantId != req.TenantId)
            throw new UnauthorizedAccessException("Tenant mismatch.");

        var exists = await _db.Patients.AnyAsync(p => p.TenantId == req.TenantId && p.PhoneNumber == req.PhoneNumber.Trim(), ct);
        if (exists)
            throw new InvalidOperationException("A patient with this phone number already exists in this tenant.");

        var patient = new Patient
        {
            Id = Guid.NewGuid(),
            FirstName = req.FirstName.Trim(),
            LastName = req.LastName.Trim(),
            PhoneNumber = req.PhoneNumber.Trim(),
            TenantId = req.TenantId,
            PrimaryBranchId = req.PrimaryBranchId,
            CreatedAt = DateTime.UtcNow
        };

        try
        {
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pg && pg.SqlState == "23505")
        {
            if (pg.ConstraintName?.Contains("PhoneNumber") == true)
                throw new InvalidOperationException("A patient with this phone number already exists in this tenant.");
            throw;
        }

        await InvalidatePatientListCache(req.TenantId, req.PrimaryBranchId);
        return patient;
    }

    public async Task<IReadOnlyList<PatientListItem>> ListAsync(Guid tenantId, Guid? branchId, CancellationToken ct = default)
    {
        if (_tenant.TenantId != null && _tenant.TenantId != tenantId)
            throw new UnauthorizedAccessException("Tenant mismatch.");

        var cacheKey = CacheKeys.PatientList(tenantId, branchId);
        var cached = await _cache.GetAsync<List<PatientListItem>>(cacheKey, ct);
        if (cached != null)
            return cached;

        var query = _db.Patients.AsNoTracking()
            .Where(p => p.TenantId == tenantId);

        if (branchId.HasValue)
            query = query.Where(p => p.PrimaryBranchId == branchId.Value);

        var list = await query
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new PatientListItem(p.Id, p.FirstName, p.LastName, p.PhoneNumber, p.TenantId, p.PrimaryBranchId, p.CreatedAt))
            .ToListAsync(ct);

        await _cache.SetAsync(cacheKey, list, TimeSpan.FromMinutes(5), ct);
        return list;
    }

    private async Task InvalidatePatientListCache(Guid tenantId, Guid? branchId)
    {
        await _cache.RemoveByPrefixAsync($"tenant:{tenantId}:patients:list:");
    }
}

public static class CacheKeys
{
    public static string PatientList(Guid tenantId, Guid? branchId) =>
        $"tenant:{tenantId}:patients:list:{(branchId.HasValue ? branchId.Value.ToString() : "all")}";
}
