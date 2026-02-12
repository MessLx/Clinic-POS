using Microsoft.EntityFrameworkCore;
using Npgsql;
using PClinicPOS.Api.Data;
using PClinicPOS.Api.Auth;
using PClinicPOS.Api.Models;

namespace PClinicPOS.Api.Services;

public class AppointmentService : IAppointmentService
{
    private readonly AppDbContext _db;
    private readonly ITenantContext _tenant;
    private readonly ICacheService _cache;
    private readonly IMessagePublisher _publisher;

    public AppointmentService(AppDbContext db, ITenantContext tenant, ICacheService cache, IMessagePublisher publisher)
    {
        _db = db;
        _tenant = tenant;
        _cache = cache;
        _publisher = publisher;
    }

    public async Task<Appointment> CreateAsync(CreateAppointmentRequest req, CancellationToken ct = default)
    {
        if (_tenant.TenantId != null && _tenant.TenantId != req.TenantId)
            throw new UnauthorizedAccessException("Tenant mismatch.");

        var appointment = new Appointment
        {
            Id = Guid.NewGuid(),
            TenantId = req.TenantId,
            BranchId = req.BranchId,
            PatientId = req.PatientId,
            StartAt = req.StartAt,
            CreatedAt = DateTime.UtcNow
        };

        try
        {
            _db.Appointments.Add(appointment);
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pg && pg.SqlState == "23505")
        {
            if (pg.ConstraintName?.Contains("BranchId") == true || pg.ConstraintName?.Contains("StartAt") == true)
                throw new InvalidOperationException("An appointment already exists for this patient at this branch and time.");
            throw;
        }

        await _cache.RemoveByPrefixAsync($"tenant:{req.TenantId}:patients:list:");
        _publisher.Publish("AppointmentCreated", new { TenantId = req.TenantId, AppointmentId = appointment.Id, BranchId = req.BranchId, PatientId = req.PatientId, StartAt = req.StartAt });
        return appointment;
    }
}
