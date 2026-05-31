using System.Net;
using System.Net.Http.Json;
using AppCore.Dto;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using WebApi;
using Xunit;

namespace UnitTest.Controllers;

public class GatesControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public GatesControllerTests(WebApplicationFactory<Program> factory)
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
    public async Task GetAllGates_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/api/gates");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<ParkingGateDto>>();
        Assert.NotNull(result);
        Assert.True(result.TotalCount > 0);
    }

    [Fact]
    public async Task GetGateByName_WhenExists_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/api/gates/name/Wjazd 1");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ParkingGateDto>();
        Assert.NotNull(result);
        Assert.Equal("Wjazd 1", result.Name);
    }

    [Fact]
    public async Task CreateGate_ReturnsCreated()
    {
        // Arrange
        var request = new CreateGateDto("Brama Testowa", "Entry", "Zachód");

        // Act
        var response = await _client.PostAsJsonAsync("/api/gates", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ParkingGateDto>();
        Assert.NotNull(result);
        Assert.Equal("Brama Testowa", result.Name);
    }

    [Fact]
    public async Task UpdateGate_WhenExists_ReturnsOk()
    {
        // Arrange - najpierw pobierzmy id istniejącej bramki
        var gatesResponse = await _client.GetAsync("/api/gates");
        var gates = await gatesResponse.Content.ReadFromJsonAsync<PagedResult<ParkingGateDto>>();
        var gateId = gates!.Items.First().Id;
        var updateRequest = new UpdateGateDto("Zaktualizowana Nazwa", "Exit");

        // Act
        var response = await _client.PutAsJsonAsync($"/api/gates/{gateId}", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ParkingGateDto>();
        Assert.NotNull(result);
        Assert.Equal("Zaktualizowana Nazwa", result.Name);
    }

    [Fact]
    public async Task ChangeStatus_ReturnsNoContent()
    {
        // Arrange
        var gatesResponse = await _client.GetAsync("/api/gates");
        var gates = await gatesResponse.Content.ReadFromJsonAsync<PagedResult<ParkingGateDto>>();
        var gateId = gates!.Items.First().Id;

        // Act
        var response = await _client.PatchAsJsonAsync($"/api/gates/{gateId}/status", false);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}
