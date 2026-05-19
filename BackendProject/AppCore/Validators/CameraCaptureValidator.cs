using AppCore.Dto;
using FluentValidation;

namespace AppCore.Validators;

public class CameraCaptureValidator : AbstractValidator<CameraCaptureDto>
{
    public CameraCaptureValidator()
    {
        RuleFor(x => x.LicensePlate)
            .NotEmpty().WithMessage("Numer rejestracyjny jest wymagany.")
            .MaximumLength(15).WithMessage("Numer rejestracyjny nie może przekraczać 15 znaków.");

        RuleFor(x => x.GateName)
            .NotEmpty().WithMessage("Nazwa bramki jest wymagana.");
    }
}

public class CreateCameraCaptureValidator : AbstractValidator<CreateCameraCaptureDto>
{
    public CreateCameraCaptureValidator()
    {
        RuleFor(x => x.LicensePlate).NotEmpty().MaximumLength(15);
        RuleFor(x => x.CaptureType).NotEmpty();
    }
}