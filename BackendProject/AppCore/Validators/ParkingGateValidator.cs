using AppCore.Dto;
using AppCore.Models;
using FluentValidation;

namespace AppCore.Validators;

public class ParkingGateValidator : AbstractValidator<CreateGateDto>
{
    public ParkingGateValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nazwa bramki jest wymagana.")
            .MaximumLength(20).WithMessage("Nazwa nie może przekraczać 20 znaków.")
            .Matches(@"^[\p{L}\s\-\d]+$").WithMessage("Nazwa zawiera niedozwolone znaki.");

        RuleFor(x => x.Location)
            .NotEmpty().WithMessage("Nazwa lokalizacji jest wymagana.")
            .MaximumLength(50).WithMessage("Lokalizacja nie może przekraczać 50 znaków.")
            .Matches(@"^[\p{L}\s\-\d]+$").WithMessage("Lokalizacja zawiera niedozwolone znaki.");

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Typ bramki jest wymagany.")
            .IsEnumName(typeof(GateType), caseSensitive: false).WithMessage("Niepoprawny typ bramki.");
    }
}

public class ParkingGateModelValidator : AbstractValidator<ParkingGate>
{
    public ParkingGateModelValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nazwa bramki jest wymagana.")
            .MaximumLength(20).WithMessage("Nazwa nie może przekraczać 20 znaków.");

        RuleFor(x => x.Location)
            .NotEmpty().WithMessage("Nazwa lokalizacji jest wymagana.")
            .MaximumLength(50).WithMessage("Lokalizacja nie może przekraczać 50 znaków.");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Niepoprawny typ bramki.");
    }
}
