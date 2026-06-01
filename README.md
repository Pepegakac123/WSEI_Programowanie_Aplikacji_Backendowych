# System Obsługi Parkingu - API dla Bramek (Zadanie 21)

Projekt realizujący backendową obsługę automatycznego systemu parkingowego (PAB26). Moduł ten dostarcza API przeznaczone do komunikacji z fizycznymi bramkami wjazdowymi i wyjazdowymi, realizując pełen proces przetwarzania sesji parkowania.

## Autor
* **Kacper Adamczyk Nr Albumu: 15606 Grupa lab1/2/PROGN**

## Link do repozytorium
[Link do repozytorium na GitHubie](https://github.com/Pepegakac123/WSEI_Programowanie_Aplikacji_Backendowych/)

## Zrealizowane funkcje (Zgodnie z wymaganiami Zadania 21)

### 1. Przetwarzanie sesji parkowania
> "Tworzenie sesji następuje, gdy auto jest rejestrowane przy wjeździe na parking, rozliczenie sesji z określeniem płatności, gdy bramka wyjazdowa rejestruje pojazd wyjeżdżający."

System w `ParkingSessionService` zarządza cyklem życia sesji. Przy wjeździe tworzony jest rekord `ParkingSession`, a przy wyjeździe system oblicza czas postoju i należną opłatę.

### 2. Obsługa wjazdu i rozpoznawanie pojazdu
> "Otwarcia braki wjazdowej po wykonaniu zdjęcia i rozpoznaniu danych auta."

Przy każdym żądaniu wjazdu system symuluje działanie inteligentnej kamery, która rozpoznaje markę i kolor pojazdu. Dane te są zapisywane w tabeli `CameraCapture` wraz z wygenerowaną ścieżką do zdjęcia.

**Snippet kodu (Symulator Kamery):**
```csharp
// Fragment z SmartCameraSimulator.cs
public static (string Brand, string Color) RecognizeDetails()
{
    var randomBrand = Brands[RandomGenerator.Next(Brands.Length)];
    var randomColor = Colors[RandomGenerator.Next(Colors.Length)];

    return (randomBrand, randomColor);
}
```


### 3. Warunkowe otwarcie bramki wyjazdowej
> "Otwarcia bramki wyjazdowej, gdy należność została uregulowana lub czas parkowania mieści się w limicie czasu darmowego."

Logika wyjazdu sprawdza, czy aktualny czas mieści się w `FreeParkingDuration` zdefiniowanym w taryfie. Jeśli nie, sprawdza czy odnotowano wpłatę.

### 4. Blokowanie wyjazdu i żądanie zapłaty
> "Blokowania przejazdu wraz z żądaniem zapłaty, jeśli nie opłacono należności lub przekroczono czas na wyjazd (po opłaceniu kierowca ma 10 minut na wyjazd)."

Przestrzeganie limitu 10 minut na opuszczenie parkingu po dokonaniu płatności.

**Snippet kodu (Logika wyjazdu):**
```csharp
// Fragment z ParkingSessionService.cs
if (currentSession is { ParkingFee: not null, PaymentTime: not null })
{
    var timeSincePayment = DateTime.Now - currentSession.PaymentTime.Value;
    if (timeSincePayment.TotalMinutes <= 10)
    {
        await CloseSessionAsync(currentSession);
        return new ParkingExitResultDto(..., GateAction.Open);
    }
}
```

### 5. Blokowanie wjazdu przy braku miejsc
> "Blokowanie wjazdu, gdy brak miejsc parkingowych."

Przed utworzeniem sesji system pobiera globalne ustawienia `TotalSpaces` i porównuje je z liczbą aktywnych sesji.

**Snippet kodu (Walidacja zajętości):**
```csharp
var totalSpaces = await unit.ParkingSettings.GetTotalSpacesAsync();
if (totalSpaces - activeSessions.Count() <= 0)
{
    return new ParkingEntryResultDto(..., GateAction.KeepClosed, Message = "Brak miejsc");
}
```

### 6. Bezpieczeństwo i Własność (Ownership)
Zgodnie z ogólnymi wytycznymi laboratoriów, operacje na zasobach są chronione.

**Snippet kodu (Sprawdzanie właściciela):**
```csharp
private void EnsureOwnershipOrAdmin(string? createdByUserId)
{
    if (currentUserService.IsAdmin) return;
    if (createdByUserId != currentUserService.UserId)
    {
        throw new UnauthorizedAccessException("Nie masz uprawnień do tego zasobu.");
    }
}
```

## Uruchomienie projektu

### Technologia
* Środowisko: **.NET 10**
* Język: **C# 13**
* Baza danych: **SQLite**

### Wymagania wstępne
* Zainstalowany .NET 10 SDK
* Narzędzie `dotnet-ef` (zainstalowane lokalnie w projekcie)

### Kroki do uruchomienia
1. Pobierz zależności:
   ```bash
   dotnet restore
   ```
2. Zainstaluj lokalne narzędzia (w tym EF Core CLI):
   ```bash
   dotnet tool restore
   ```
3. Wykonaj migrację bazy danych, aby utworzyć tabele:
   ```bash
   dotnet ef database update --project Infrastructure --startup-project WebApi
   ```
4. Uruchom aplikację:
   ```bash
   dotnet run --project WebApi
   ```

Aplikacja w trybie `Development` automatycznie zainicjuje dane (Seeder) dla użytkowników, ról, taryf i bramek.

## Testy

Projekt posiada testy jednostkowe i integracyjne weryfikujące logikę Zadania 21 (m.in. naliczanie opłat, limity czasu, blokowanie przy braku miejsc).

Uruchomienie testów:
```bash
dotnet test
```
