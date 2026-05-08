# Instrukcja Laboratorium 5 - Parking (09.05.2026)

Niniejszy dokument zawiera szczegółowe instrukcje dotyczące implementacji funkcjonalności rejestracji obrazów z kamer (Camera Capture) w systemie parkingowym, obsługi wyjątków oraz globalnego zarządzania błędami.

---

## 1. Rozszerzenie Modelu Danych

Pierwszym krokiem jest powiązanie bramki parkingowej z rejestracjami z kamer. Każda bramka może posiadać wiele zarejestrowanych obrazów.

### Plik: `AppCore/Models/ParkingGate.cs`
Dodaj kolekcję `CameraCaptures` do klasy `ParkingGate`.

```csharp
// Lokalizacja: AppCore/Models/ParkingGate.cs

public class ParkingGate : EntityBase
{
    // ... istniejące właściwości (Name, Type, Location, IsOperational)

    // NOWOŚĆ: Kolekcja przechowująca migawki z kamery powiązane z tą bramką
    public ICollection<CameraCapture> CameraCaptures { get; set; } = new List<CameraCapture>();
}
```
**Wyjaśnienie:** `ICollection<CameraCapture>` definiuje relację jeden-do-wielu. Jedna bramka może mieć wiele zapisanych "przechwyceń" (captures).

---

## 2. Definicja DTO i Walidacja

Musimy zdefiniować obiekt, który będzie przesyłany przez klienta podczas dodawania nowego obrazu.

### Plik: `AppCore/Dto/CreateCameraCaptureDto.cs`
Utwórz nowy plik dla DTO zapisu.

```csharp
// Lokalizacja: AppCore/Dto/CreateCameraCaptureDto.cs

namespace AppCore.Dto;

public record CreateCameraCaptureDto(
    string LicensePlate,
    string Brand,
    string Color,
    string? ImagePath,
    string CaptureType // Wjazd lub Wyjazd
);
```

### Plik: `AppCore/Validators/CreateCameraCaptureValidator.cs`
Dodaj walidację dla powyższego DTO.

```csharp
// Lokalizacja: AppCore/Validators/CreateCameraCaptureValidator.cs

using AppCore.Dto;
using FluentValidation;

namespace AppCore.Validators;

public class CreateCameraCaptureValidator : AbstractValidator<CreateCameraCaptureDto>
{
    public CreateCameraCaptureValidator()
    {
        RuleFor(x => x.LicensePlate).NotEmpty().MaximumLength(15);
        RuleFor(x => x.CaptureType).NotEmpty();
    }
}
```

---

## 3. Obsługa Wyjątków - Definicja

Zdefiniujemy własny wyjątek, który będzie rzucany, gdy np. nie znajdziemy bramki o podanym ID.

### Plik: `AppCore/Exceptions/GateNotFoundException.cs`
Utwórz katalog `Exceptions` w `AppCore` i dodaj klasę.

```csharp
// Lokalizacja: AppCore/Exceptions/GateNotFoundException.cs

namespace AppCore.Exceptions;

public class GateNotFoundException : Exception
{
    public GateNotFoundException(string msg) : base(msg)
    {
    }
}
```

---

## 4. Warstwa Repozytoriów

Musimy dodać możliwość zapisywania obiektów `CameraCapture`.

### Plik: `AppCore/Repositories/ICameraCaptureRepository.cs`
```csharp
// Lokalizacja: AppCore/Repositories/ICameraCaptureRepository.cs

using AppCore.Models;

namespace AppCore.Repositories;

public interface ICameraCaptureRepository : IGenericRepositoryAsync<CameraCapture>
{
}
```

### Plik: `Infrastructure/Repositories/InMemoryCameraCaptureRepository.cs`
```csharp
// Lokalizacja: Infrastructure/Repositories/InMemoryCameraCaptureRepository.cs

using AppCore.Models;
using AppCore.Repositories;

namespace Infrastructure.Repositories;

public class InMemoryCameraCaptureRepository : MemoryGenericRepository<CameraCapture>, ICameraCaptureRepository
{
}
```

### Plik: `AppCore/Repositories/IParkingUnitOfWork.cs`
Zaktualizuj interfejs Unit of Work.

```csharp
// Lokalizacja: AppCore/Repositories/IParkingUnitOfWork.cs

public interface IParkingUnitOfWork
{
    // ... inne repozytoria
    ICameraCaptureRepository CameraCaptures { get; } // NOWOŚĆ
    // ... metody SaveChanges
}
```

---

## 5. Logika Biznesowa (Serwis)

Serwis będzie odpowiedzialny za logikę dodawania obrazu i rzucanie wyjątków.

### Plik: `AppCore/Services/IParkingGateService.cs`
Dodaj nowe metody do interfejsu.

```csharp
// Lokalizacja: AppCore/Services/IParkingGateService.cs

using AppCore.Dto;

namespace AppCore.Services;

public interface IParkingGateService
{
    // ... istniejące metody
    Task<CameraCaptureDto> AddCapture(Guid gateId, CreateCameraCaptureDto dto);
    Task<IEnumerable<CameraCaptureDto>> GetCaptures(Guid gateId);
    Task DeleteCapture(Guid gateId, Guid captureId);
}
```

### Plik: `Infrastructure/Services/MemoryParkingGateService.cs`
Implementacja logiki w serwisie.

```csharp
// Lokalizacja: Infrastructure/Services/MemoryParkingGateService.cs

public async Task<CameraCaptureDto> AddCapture(Guid gateId, CreateCameraCaptureDto dto)
{
    var gate = await unit.Gates.FindByIdAsync(gateId);
    
    // Sprawdzenie czy bramka istnieje - jeśli nie, rzucamy nasz wyjątek
    if (gate == null)
    {
        throw new GateNotFoundException($"Gate with id={gateId} not found!");
    }

    var capture = new CameraCapture
    {
        Id = Guid.NewGuid(),
        LicensePlate = dto.LicensePlate,
        DetectedBrand = dto.Brand,
        DetectedColor = dto.Color,
        ImagePath = dto.ImagePath,
        CaptureType = Enum.Parse<CaptureType>(dto.CaptureType, true),
        CapturedAt = DateTime.UtcNow,
        GateName = gate.Name
    };

    await unit.CameraCaptures.AddAsync(capture);
    gate.CameraCaptures.Add(capture);
    
    await unit.SaveChangesAsync();

    return _mapper.Map<CameraCaptureDto>(capture);
}
```

---

## 6. Kontroler API

Dodajemy endpointy, które pozwolą na komunikację z systemem przez HTTP.

### Plik: `WebApi/Controllers/GatesController.cs`

```csharp
// Lokalizacja: WebApi/Controllers/GatesController.cs

[HttpPost("{gateId:guid}/captures")]
public async Task<IActionResult> AddCameraCapture(
    [FromRoute] Guid gateId,
    [FromBody] CreateCameraCaptureDto dto)
{
    // Wywołujemy serwis - jeśli rzuci wyjątek, zostanie on obsłużony przez Global Handler
    var capture = await service.AddCapture(gateId, dto);
    
    return CreatedAtAction(
        nameof(GetCaptures),
        new { gateId },
        capture
    );
}

[HttpGet("{gateId:guid}/captures")]
public async Task<IActionResult> GetCaptures([FromRoute] Guid gateId)
{
    var captures = await service.GetCaptures(gateId);
    return Ok(captures);
}
```

---

## 7. Globalna Obsługa Wyjątków

Zamiast pisać `try-catch` w każdym kontrolerze, stworzymy mechanizm, który automatycznie przechwyci nasze wyjątki i zwróci czytelny błąd JSON.

### Plik: `WebApi/Middleware/ProblemDetailsExceptionHandler.cs`

```csharp
// Lokalizacja: WebApi/Middleware/ProblemDetailsExceptionHandler.cs

using AppCore.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace WebApi.Middleware;

public class ProblemDetailsExceptionHandler(
    ProblemDetailsFactory factory, ILogger<ProblemDetailsExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        // Sprawdzamy czy to nasz specyficzny wyjątek
        if (exception is GateNotFoundException)
        {
            logger.LogInformation($"Obsłużono wyjątek: {exception.Message}");
            
            var problem = factory.CreateProblemDetails(
                context,
                StatusCodes.Status400BadRequest,
                title: "Błąd serwisu",
                detail: exception.Message
            );

            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(problem, cancellationToken);
            return true; // Informujemy, że błąd został obsłużony
        }
        
        return false; // Przekaż do następnego handlera (lub domyślnego)
    }
}
```

### Plik: `WebApi/Program.cs`
Zarejestruj handler w potoku przetwarzania.

```csharp
// Lokalizacja: WebApi/Program.cs

var builder = WebApplication.CreateBuilder(args);

// ...
builder.Services.AddExceptionHandler<ProblemDetailsExceptionHandler>();    
builder.Services.AddProblemDetails();
// ...

var app = builder.Build();

// ...
app.UseExceptionHandler(); // Musi być przed mapowaniem kontrolerów
app.MapControllers();
// ...
```

---

## 8. Testowanie (WebApi.http)

Możesz przetestować endpointy dodając poniższe żądania do pliku `.http`.

```http
### Dodanie obrazu do istniejącej bramki
POST {{WebApi_HostAddress}}/api/gates/{{gateId}}/captures
Content-Type: application/json

{
  "licensePlate": "KR 12345",
  "brand": "Toyota",
  "color": "Silver",
  "imagePath": "/storage/cam1/img123.jpg",
  "captureType": "In"
}

### Pobranie obrazów dla bramki
GET {{WebApi_HostAddress}}/api/gates/{{gateId}}/captures

### Test błędu (nieistniejąca bramka)
POST {{WebApi_HostAddress}}/api/gates/00000000-0000-0000-0000-000000000000/captures
Content-Type: application/json

{
  "licensePlate": "ERR",
  "captureType": "In"
}
```

---

## Podsumowanie zmian
1. **Model**: Relacja bramka -> obrazy.
2. **Wyjątki**: Autorski `GateNotFoundException` zamiast standardowych błędów.
3. **Global Handler**: Czysty kod w kontrolerach dzięki centralizacji obsługi błędów.
4. **Service**: Rozszerzona logika o operacje na migawkach.
