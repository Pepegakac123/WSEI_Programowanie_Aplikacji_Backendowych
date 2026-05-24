namespace AppCore.Exceptions;

public class InvalidCredentialsException(string message = "Nieprawidłowy email lub hasło.") : Exception(message);
