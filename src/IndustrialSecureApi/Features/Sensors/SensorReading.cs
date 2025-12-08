namespace IndustrialSecureApi.Features.Sensors;

public record SensorReading(
    Guid Id,
    string Tag,
    double Value,
    DateTime Timestamp
);