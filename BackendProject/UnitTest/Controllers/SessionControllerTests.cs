using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using AppCore.Dto;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using WebApi;
using WebApi.Controllers;
using Xunit;

namespace UnitTest.Controllers;


public class SessionsControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public SessionsControllerTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddSingleton<IPolicyEvaluator, FakePolicyEvaluator>();
            });
        }).CreateClient();
    }

    [Fact]
    public async Task PostEntry_WithValidData_ReturnsOk()
    {

        var request = new ParkingEntryAndExitRequest("Wjazd 1", "E2E-123");
        
        var response = await _client.PostAsJsonAsync("/api/sessions/entry", request);
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var result = await response.Content.ReadFromJsonAsync<ParkingEntryResultDto>();
        Assert.NotNull(result);
        Assert.Equal("Wjazd 1", result.GateName);
        Assert.NotNull(result.Vehicle);
        Assert.Equal("E2E-123", result.Vehicle.LicensePlate);
    }
    
    [Fact]
    public async Task PostEntry_WithNonExistentGate_ReturnsNotFound()
    {
        var request = new ParkingEntryAndExitRequest("Widmo Brama", "E2E-123");
        
        var response = await _client.PostAsJsonAsync("/api/sessions/entry", request);
        
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PostExit_WithWrongGateType_ReturnsBadRequest()
    {
        var request = new ParkingEntryAndExitRequest("Wjazd 1", "E2E-123");
        
        var response = await _client.PostAsJsonAsync("/api/sessions/exit", request);
        
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}

public class FakePolicyEvaluator : IPolicyEvaluator
{
    public Task<AuthenticateResult> AuthenticateAsync(AuthorizationPolicy policy, HttpContext context)
    {

        var principal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "test-user-id"),
            new Claim(ClaimTypes.Role, "GateController"), 
            new Claim(ClaimTypes.Role, "Administrator")  
        }, "TestAuthType"));

        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal, "TestAuthType")));
    }

    public Task<PolicyAuthorizationResult> AuthorizeAsync(AuthorizationPolicy policy, AuthenticateResult authenticationResult, HttpContext context, object? resource)
    {
        // Zawsze zwracamy sukces - każda polityka (np. AdminOnly) zostanie zignorowana w testach
        return Task.FromResult(PolicyAuthorizationResult.Success());
    }
}