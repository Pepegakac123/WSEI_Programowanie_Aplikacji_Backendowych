using AppCore.Dto;
using AppCore.Models;
using FluentValidation;

namespace AppCore.Validators;

public class ParkingSessionValidator : AbstractValidator<ParkingSession>
{
    public ParkingSessionValidator()
    {
        RuleFor(x => x.VehicleId)
            .NotEmpty().WithMessage("Identyfikator pojazdu jest wymagany.");

        RuleFor(x => x.GateName)
            .NotEmpty().WithMessage("Nazwa bramki jest wymagana.")
            .MaximumLength(20).WithMessage("Nazwa bramki nie może przekraczać 20 znaków.");

        RuleFor(x => x.EntryTime)
            .NotEmpty().WithMessage("Czas wjazdu jest wymagany.");

        RuleFor(x => x.ExitTime)
            .GreaterThan(x => x.EntryTime)
            .When(x => x.ExitTime.HasValue)
            .WithMessage("Czas wyjazdu musi być późniejszy niż czas wjazdu.");

        RuleFor(x => x.ParkingFee)
            .GreaterThanOrEqualTo(0)
            .When(x => x.ParkingFee.HasValue)
            .WithMessage("Opłata parkingowa nie może być ujemna.");

        RuleFor(x => x.PaymentTime)
            .GreaterThanOrEqualTo(x => x.EntryTime)
            .When(x => x.PaymentTime.HasValue)
            .WithMessage("Czas płatności nie może być wcześniejszy niż czas wjazdu.");
    }
}

public class ParkingEntryAndExitRequestValidator : AbstractValidator<ParkingEntryAndExitRequest>
{
    public ParkingEntryAndExitRequestValidator()
    {
        RuleFor(x => x.GateName)
            .NotEmpty().WithMessage("Nazwa bramki jest wymagana.");

        RuleFor(x => x.LicensePlate)
            .NotEmpty().WithMessage("Numer rejestracyjny jest wymagany.")
            .MaximumLength(15).WithMessage("Numer rejestracyjny nie może przekraczać 15 znaków.");
    }
}
