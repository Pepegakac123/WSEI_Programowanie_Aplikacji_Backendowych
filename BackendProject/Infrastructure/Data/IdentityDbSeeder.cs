using AppCore.Users;
using Infrastructure.EntityFramework.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Data;

public class IdentityDbSeeder(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager, ILogger<IdentityDbSeeder> logger): IDataSeeder
{
    public int Order => 1;

    public async Task SeedAsync()
    {
        await SeedRolesAsync();
        await SeedUsersAsync();
    }

    private async Task SeedRolesAsync()
    {
        var roles = new[]
        {
            new AppRole(UserRole.Administrator.ToString(), "Pełny dostęp do systemu administracji parkingiem."),
            new AppRole(UserRole.GateController.ToString(), "Obsługa i monitorowanie bramek wjazdowych/wyjazdowych."),
            new AppRole(UserRole.Driver.ToString(), "Kierowca korzystający z miejsc parkingowych.")
        };

        foreach (var role in roles)
        {
            if (await roleManager.RoleExistsAsync(role.Name!))
                continue;

            var result = await roleManager.CreateAsync(role);
            if (!result.Succeeded)
            {
                logger.LogError("Błąd tworzenia roli {Role}: {Errors}", 
                    role.Name, string.Join("; ", result.Errors.Select(e => e.Description)));
            }
        }
    }
    private async Task SeedUsersAsync()
    {
        var users = new[]
        {
            new SeedUserDto("F5BADE14-6CC8-42A2-9A44-9842DA2D9280", "admin@parking.pl", "Adam", "Admin", "Admin@123!", UserRole.Administrator),
            new SeedUserDto("93A7FFDD-057F-4021-9C68-FE06951FFA65", "jan.kowalski@parking.pl", "Jan", "Kowalski", "Staff@123!", UserRole.GateController),
            new SeedUserDto("3D4769E2-1C75-43E1-A5BB-1F71C68E9F57", "anna.nowak@parking.pl", "Anna", "Nowak", "Driver@123!", UserRole.Driver)
        };

        foreach (var seedUser in users)
        {
            // Próbujemy znaleźć po ID, bo to jest najpewniejsze jeśli NormalizedEmail jest NULL
            var user = await userManager.FindByIdAsync(seedUser.Id);
            
            // Jeśli nie znaleziono po ID, spróbujmy po Email (może ID się zmieniło?)
            if (user is null)
            {
                user = await userManager.FindByEmailAsync(seedUser.Email);
            }
            
            if (user is not null)
            {
                // Upewnijmy się, że pola znormalizowane są uzupełnione
                if (string.IsNullOrEmpty(user.NormalizedEmail) || string.IsNullOrEmpty(user.NormalizedUserName))
                {
                    user.NormalizedEmail = userManager.NormalizeEmail(user.Email!);
                    user.NormalizedUserName = userManager.NormalizeName(user.UserName!);
                    await userManager.UpdateAsync(user);
                }
                continue;
            }

            user = new AppUser
            {
                Id = seedUser.Id,
                UserName = seedUser.Email,
                Email = seedUser.Email,
                FirstName = seedUser.FirstName,
                LastName = seedUser.LastName,
                Status = SystemUserStatus.Active,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow,
                NormalizedEmail = userManager.NormalizeEmail(seedUser.Email),
                NormalizedUserName = userManager.NormalizeName(seedUser.Email)
            };

            var createResult = await userManager.CreateAsync(user, seedUser.Password);
            if (createResult.Succeeded)
            {
                await userManager.AddToRoleAsync(user, seedUser.Role.ToString());
                logger.LogInformation("Utworzono użytkownika {Email} z rolą {Role}.", user.Email, seedUser.Role);
            }
            else
            {
                logger.LogError("Błąd tworzenia użytkownika {Email}: {Errors}", 
                    user.Email, string.Join("; ", createResult.Errors.Select(e => e.Description)));
            }
        }
    }
}
internal record SeedUserDto(string Id, string Email, string FirstName, string LastName, string Password, UserRole Role);