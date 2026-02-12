using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using PClinicPOS.Api;
using PClinicPOS.Api.Data;
using Xunit;

namespace PClinicPOS.Tests;

public class AppFactory : WebApplicationFactory<ProgramEntry>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        // Program uses InMemory when Environment is "Testing", so no need to replace DbContext here.
    }
}

public class ApiSmokeTests : IClassFixture<AppFactory>
{
    private readonly AppFactory _factory;

    public ApiSmokeTests(AppFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Login_ReturnsToken_ForSeededUser()
    {
        using var client = _factory.CreateClient();
        var res = await client.PostAsJsonAsync("/api/auth/login", new { email = "admin@peaaura.local", password = DataSeeder.DefaultPassword });
        Assert.True(res.IsSuccessStatusCode, await res.Content.ReadAsStringAsync());
        var body = await res.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(body?.Token);
        Assert.NotEmpty(body.Token);
    }

    private class LoginResponse
    {
        public string Token { get; set; } = "";
        public object? User { get; set; }
    }
}
