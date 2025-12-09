namespace IndustrialSecureApi.Features.Sensors.Dtos;

public record CreateSensorReadingDto
{
    public string Tag { get; init; } = string.Empty;
    public double Value { get; init; }
    public DateTime Timestamp { get; init; }
}