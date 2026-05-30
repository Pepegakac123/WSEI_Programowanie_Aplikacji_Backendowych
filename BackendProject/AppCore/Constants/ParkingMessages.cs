namespace AppCore.Constants;

public static class ParkingMessages
{
    public const string NoAvailableSpaces = "Brak miejsc parkingowych";
    public const string Welcome = "Bramka otwarta, zapraszamy";
    public const string Goodbye = "Bramka otwarta, do zobaczenia";
    public const string ForbbidenEntry = "Brak aktywnej sesji dla tego pojazdu. Skontaktuj się z obsługą.";
    public static string FeeMsg(decimal fee) => $"Proszę opłacić {fee} zł";
    public static string GoodbyeWithPaymentConfirmationMsg(decimal fee) => $"Bramka otwarta, Opłata wyniosła {fee} zł do zobaczenia ";
    
}