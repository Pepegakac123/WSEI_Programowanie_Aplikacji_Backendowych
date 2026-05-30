# Plan Rozbudowy Systemu Parkingowego

Ten dokument zawiera szczegółowy plan implementacji brakujących funkcjonalności zgodnie z wymaganiami projektu.

## 1. Rozszerzenie Modelu Danych (AppCore & Infrastructure)

### A. Tożsamość i Własność (Identity)
Zgodnie z wymaganiem: "każdy dodawany kontakt posiadał informację o użytkowniku, który go dodał".
- **Zmiana w `EntityBase.cs`**: Dodanie pola `public string? CreatedByUserId { get; set; }`.
- **Aktualizacja `AppUser.cs`**: Upewnienie się, że relacje są poprawne (opcjonalne, można zostać przy stringu dla prostoty).
- **Aktualizacja `ParkingDbContext.cs`**: Konfiguracja nowych pól, jeśli to konieczne.
- **Migracja**: Wygenerowanie nowej migracji EF (`AddOwnershipFields`).

### B. Zarządzanie Pojemnością Parkingu
- **Opcja 1**: Nowa encja `ParkingSettings` przechowująca `TotalSpaces`.
- **Opcja 2**: Pole w `appsettings.json`.
- **Decyzja**: Implementacja `ParkingSettings` w bazie danych (tabela z jednym rekordem) dla zachowania spójności z "Clean Architecture".

## 2. Logika Biznesowa (AppCore - Services)

### A. `IParkingSessionService` i `ParkingSessionService`
Implementacja głównej logiki przetwarzania wjazdów i wyjazdów:
- `ProcessEntry(string gateName, string licensePlate)`:
    - Sprawdzenie czy pojazd o takim numerze istnieje (jeśli nie - rejestracja).
    - Sprawdzenie dostępności miejsc (Liczba aktywnych sesji < `TotalSpaces`).
    - Tworzenie nowej sesji (`ParkingSession`).
    - Zwrócenie wyniku (Otwórz bramkę / Brak miejsc).
- `ProcessExit(string gateName, string licensePlate)`:
    - Znalezienie aktywnej sesji dla danego pojazdu.
    - Obliczenie opłaty na podstawie `ParkingTariff`.
    - Sprawdzenie czy opłacono (lub czy mieści się w darmowym czasie).
    - Obsługa limitu 10 minut na wyjazd po opłaceniu.
    - Zwrócenie wyniku (Otwórz / Zapłać / Przekroczono czas).

### B. Kalkulator Opłat (Logika pomocnicza)
- Pobranie aktywnej taryfy (`ParkingTariff`).
- Obliczenie: `(CzasPostoju - DarmowyCzas) * StawkaGodzinowa`.
- Uwzględnienie `DailyMaxRate`.

## 3. Warstwa API (WebApi - Controllers)

### A. `SessionsController`
Nowe endpointy dla bramek:
- `POST /api/sessions/entry`: Przyjmuje dane z bramki wjazdowej.
- `POST /api/sessions/exit`: Przyjmuje dane z bramki wyjazdowej.
- `GET /api/sessions/active`: Lista aktywnych parkowań (dla administratora).
- `POST /api/sessions/{id}/pay`: Symulacja płatności.

### B. Zabezpieczenia (JWT & Authorization)
- Zastosowanie `[Authorize]` na endpointach.
- Implementacja filtrów lub logiki w serwisach sprawdzającej właściciela (`CreatedByUserId == currentUserId || isAdmin`).

## 4. Inicjalizacja Danych (Infrastructure - Data)

### A. `ParkingDataSeeder`
Nowy seeder implementujący `IDataSeeder`:
- Przykładowe bramki (Wjazd 1, Wjazd 2, Wyjazd 1).
- Przykładowa taryfa (np. 15 min gratis, 5 zł/h).
- Początkowe ustawienia parkingu (np. 50 miejsc).

## 5. Testy (UnitTest)

### A. Testy Integracyjne
- Scenariusz: Wjazd auta -> Sprawdzenie stanu zajętości.
- Scenariusz: Wyjazd przed upływem darmowego czasu -> Bramka otwarta.
- Scenariusz: Wyjazd po czasie -> Bramka zamknięta, żądanie zapłaty.
- Scenariusz: Zapłata i wyjazd w ciągu 10 minut -> Bramka otwarta.

### B. Testy E2E (WebApi)
- Testy endpointów `entry` i `exit` z użyciem `HttpClient`.

## 6. Dokumentacja (README.md)
- Opis architektury.
- Instrukcja uruchomienia.
- Przykłady żądań API.
- Opis ról i uprawnień.

---
**Uwagi techniczne:**
- Należy pamiętać o rejestracji nowych serwisów w `AppCoreModule.cs` i `ParkingInfrastructureModule.cs`.
- Wymagane jest użycie AutoMappera do mapowania sesji na DTO (już częściowo przygotowane).
