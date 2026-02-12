using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PClinicPOS.Api.Data;
using PClinicPOS.Api.Services;
using PClinicPOS.Api.Auth;
using StackExchange.Redis;

namespace PClinicPOS.Api;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddCors(o => o.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

        var isTesting = builder.Environment.EnvironmentName == "Testing";
        builder.Services.AddDbContext<AppDbContext>(options =>
        {
            if (isTesting)
                options.UseInMemoryDatabase("TestDb");
            else
                options.UseNpgsql(builder.Configuration.GetConnectionString("Default"));
        });

        var redisConn = builder.Configuration.GetConnectionString("Redis");
        if (!string.IsNullOrEmpty(redisConn))
        {
            builder.Services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConn));
            builder.Services.AddScoped<ICacheService, RedisCacheService>();
        }
        else
            builder.Services.AddScoped<ICacheService, NoOpCacheService>();

        builder.Services.AddSingleton<IMessagePublisher>(sp =>
        {
            var connStr = builder.Configuration.GetConnectionString("RabbitMQ");
            return string.IsNullOrEmpty(connStr) ? new NoOpMessagePublisher() : new RabbitMqPublisher(connStr);
        });

        var jwtKey = builder.Configuration["Jwt:Key"] ?? "dev-secret-key-min-32-chars-long!!";
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(o =>
            {
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });
        builder.Services.AddAuthorization(o =>
        {
            o.AddPolicy("Patients:Create", p => p.RequireRole("Admin", "User"));
            o.AddPolicy("Patients:View", p => p.RequireRole("Admin", "User", "Viewer"));
            o.AddPolicy("Appointments:Create", p => p.RequireRole("Admin", "User"));
        });
        builder.Services.AddScoped<ITenantContext, TenantContext>();
        builder.Services.AddScoped<IJwtService, JwtService>();
        builder.Services.AddScoped<IPatientService, PatientService>();
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<IAppointmentService, AppointmentService>();
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new() { Title = "P-Clinic-POS API", Version = "v1" });
            c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.OpenApiSecurityScheme
            {
                In = Microsoft.OpenApi.ParameterLocation.Header,
                Name = "Authorization",
                Description = "JWT Bearer token",
                Type = Microsoft.OpenApi.SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT"
            });
            c.AddSecurityRequirement(document => new()
            {
                [new Microsoft.OpenApi.OpenApiSecuritySchemeReference("Bearer", document)] = []
            });
        });

        var app = builder.Build();
        app.UseMiddleware<Middleware.ExceptionMiddleware>();

        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            if (db.Database.IsRelational())
                db.Database.Migrate();
            else
                await db.Database.EnsureCreatedAsync();
            await DataSeeder.SeedAsync(db);
        }

        app.UseCors();
        app.UseSwagger();
        app.UseSwaggerUI();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        await app.RunAsync();
    }
}
