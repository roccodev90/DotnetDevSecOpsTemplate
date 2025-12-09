using System.Net;
using System.Net.Http.Json;
using IndustrialSecureApi.Features.Sensors.Dtos;
using IndustrialSecureApi.Tests.Integration.TestHelpers;
using Xunit;

namespace IndustrialSecureApi.Tests.Integration;

public class ApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public ApiTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task POST_readings_WithValidData_Returns201()
    {
        // Arrange
        var dto = new CreateSensorReadingDto
        {
            Tag = "TAG001",
            Value = 25.5,
            Timestamp = DateTime.UtcNow
        };

        // Act
        var response = await _client.PostAsJsonAsync("/readings", dto);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }
}