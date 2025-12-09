using FluentValidation;
using IndustrialSecureApi.Features.Sensors.Dtos;

namespace IndustrialSecureApi.Features.Sensors.Validators;

public class CreateSensorReadingDtoValidator : AbstractValidator<CreateSensorReadingDto>
{
    public CreateSensorReadingDtoValidator()
    {
        RuleFor(x => x.Tag)
            .NotEmpty().WithMessage("Tag è obbligatorio")
            .MaximumLength(100).WithMessage("Tag non può superare 100 caratteri");

        RuleFor(x => x.Value)
            .GreaterThanOrEqualTo(-50).WithMessage("Value deve essere >= -50")
            .LessThanOrEqualTo(200).WithMessage("Value deve essere <= 200");

        RuleFor(x => x.Timestamp)
            .NotEmpty().WithMessage("Timestamp è obbligatorio");
    }
}