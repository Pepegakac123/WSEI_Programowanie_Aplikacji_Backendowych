namespace AppCore.Utils;

public static class SmartCameraSimulator
{
    private static readonly Random RandomGenerator = new();

    private static readonly string[] Brands = 
    {
        "Toyota", "Skoda", "Volkswagen", "Ford", "Kia", 
        "Hyundai", "BMW", "Audi", "Mercedes-Benz", "Volvo",
        "Renault", "Peugeot", "Opel", "Mazda", "Honda"
    };

    private static readonly string[] Colors = 
    {
        "Czarny", "Biały", "Srebrny", "Szary", "Czerwony", 
        "Niebieski", "Granatowy", "Zielony", "Żółty", "Brązowy"
    };

    /// <summary>
    /// Symuluje rozpoznanie detali pojazdu przez inteligentną kamerę.
    /// </summary>
    /// <returns>Tuple zawierająca losową markę i kolor.</returns>
    public static (string Brand, string Color) RecognizeDetails()
    {
        var randomBrand = Brands[RandomGenerator.Next(Brands.Length)];
        var randomColor = Colors[RandomGenerator.Next(Colors.Length)];

        return (randomBrand, randomColor);
    }
}