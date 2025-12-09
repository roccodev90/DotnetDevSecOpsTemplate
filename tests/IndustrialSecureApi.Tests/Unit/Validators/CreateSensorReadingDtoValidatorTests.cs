using FluentValidation.TestHelper;
using IndustrialSecureApi.Features.Sensors.Dtos;
using IndustrialSecureApi.Features.Sensors.Validators;

namespace IndustrialSecureApi.Tests.Unit.Validators;

public class CreateSensorReadingDtoValidatorTests
{
    [Fact]
    public void Validate_ValueLessThanMinus50_ShouldFail()
    {
        // Arrange
        var validator = new CreateSensorReadingDtoValidator();
        var dto = new CreateSensorReadingDto
        {
            Tag = "TAG001",
            Value = -51, // < -50
            Timestamp = DateTime.UtcNow
        };

        // Act
        var result = validator.Validate(dto);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Value");
    }
}