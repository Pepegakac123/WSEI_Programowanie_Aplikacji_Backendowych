using AppCore.Dto;
using FluentValidation;

namespace AppCore.Validators;

public class CreateTariffValidator : AbstractValidator<CreateTariffDto>
{
    public CreateTariffValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nazwa taryfy jest wymagana.")
            .MaximumLength(50).WithMessage("Nazwa nie może przekraczać 50 znaków.");

        RuleFor(x => x.FreeMinutes)
            .GreaterThanOrEqualTo(0).WithMessage("Liczba darmowych minut nie może być ujemna.");

        RuleFor(x => x.HourlyRate)
            .GreaterThanOrEqualTo(0).WithMessage("Stawka godzinowa nie może być ujemna.");

        RuleFor(x => x.DailyMaxRate)
            .GreaterThanOrEqualTo(x => x.HourlyRate).WithMessage("Maksymalna stawka dzienna powinna być większa lub równa stawce godzinowej.");
    }
}
