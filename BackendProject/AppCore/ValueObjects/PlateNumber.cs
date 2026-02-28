using AppCore.Exceptions;

namespace AppCore.ValueObjects;

public class PlateNumber
{
    public string Value { get; private set; }

    private PlateNumber(string input)
    {
        Value = input;
    }

    public static PlateNumber Of(string input)
    {
        if (input.Length != 8)
        {
            throw new NotValidPlateNumberException("Niepoprawna długość numeru rejestacyjnego");
        }

        return new PlateNumber(input);
    }
}