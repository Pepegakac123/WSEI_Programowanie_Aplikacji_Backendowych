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
            .IsEnumName(typeof(GateType), caseSensitive: false).WithMessage("Niepoprawny typ bramki.");
    }
}
