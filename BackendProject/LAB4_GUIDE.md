# Instrukcja do Laboratorium 4 - Walidacja i Mapowanie

Ten przewodnik pomoże Ci przejść przez zadania dotyczące walidacji DTO (FluentValidation) oraz automatycznego mapowania (AutoMapper).

---

## KROK 1: Instalacja paczek NuGet

W pierwszej kolejności musisz zainstalować wymagane biblioteki w projekcie **AppCore**.

1. Otwórz terminal w folderze projektu `AppCore`.
2. Uruchom komendy:
   ```bash
   dotnet add package FluentValidation.AspNetCore
   dotnet add package AutoMapper
   ```
3. W projekcie **WebApi** (gdzie następuje rejestracja usług):
   ```bash
   dotnet add package AutoMapper.Extensions.Microsoft.DependencyInjection
   ```

---

## KROK 2: Walidacja DTO (FluentValidation)

### 1. Folder dla walidatorów
W projekcie `AppCore` utwórz katalog `Validators`.

### 2. ParkingGateValidator
Utwórz plik `ParkingGateValidator.cs`. Uzupełnij go o reguły dla `Location` i `Type`.
*   **Location**: max 50 znaków, te same znaki co w Name.
*   **Type**: musi pasować do wartości w `GateType`.

### 3. Pozostałe walidatory
Zdefiniuj walidatory dla:
*   `CreateTariffDto`: (np. Name niepuste, stawki > 0).
*   `CameraCaptureDto`: (np. poprawny numer rejestracyjny).

### 4. Rejestracja walidatorów (Moduł AppCore)
W `AppCore` utwórz klasę statyczną `AppCoreModule` (np. w folderze `Extensions` lub `Modules`), która pozwoli na czystą rejestrację usług w `Program.cs`.

---

## KROK 3: AutoMapper (Opcjonalnie)

### 1. Profil mapowania
W `AppCore` utwórz katalog `Mappers` i plik `GateMappingProfile.cs`.
Pamiętaj o mapowaniu `Enum` na `string` za pomocą `.ToString()`.

### 2. Użycie w Serwisie
Zmień konstruktor `MemoryParkingGateService`, aby przyjmował `IMapper`. Zamień ręczne tworzenie obiektów `new ParkingGateDto(...)` na `_mapper.Map<ParkingGateDto>(entity)`.

---

## KROK 4: Integracja w WebApi

### 1. Program.cs
Zarejestruj moduł `AppCore`:
```csharp
builder.Services.AddAppCoreModule(builder.Configuration);
```

### 2. GatesController
Upewnij się, że metody:
*   `GetGate` zwraca `NotFound()`, gdy ID nie istnieje.
*   `CreateGate` używa `CreatedAtAction`.

---

## KROK 5: Testowanie (Plik .http)

Utwórz plik `WebApi/WebApi.http` i przygotuj zestaw testów:
1. `GET` wszystkich bramek.
2. `POST` poprawnej bramki (sprawdź nagłówek `Location`).
3. `POST` błędnej bramki (puste pola) -> oczekiwany status **400 Bad Request**.
4. `PUT` (Edycja) – zdefiniuj nowe DTO `UpdateGateDto` i walidator do niego.

---

## KROK 6: Zadanie domowe (Edycja)
1. Stwórz `UpdateGateDto` (tylko Name i Type).
2. Dodaj metodę `HttpPut` w `GatesController`.
3. Pamiętaj: najpierw sprawdź czy bramka istnieje (`404`), potem aktualizuj pola i zwróć `200 OK`.
