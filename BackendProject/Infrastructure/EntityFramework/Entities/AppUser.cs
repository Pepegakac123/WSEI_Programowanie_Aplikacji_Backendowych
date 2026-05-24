using AppCore.Users;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.EntityFramework.Entities;

public class AppRole : IdentityRole
{
    public string? Description { get; set; }

    public AppRole() { }
    public AppRole(string roleName, string? description = null)
        : base(roleName)
    {
        Description = description;
    }
}
public class AppUser : IdentityUser, ISystemUser
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public string FullName => $"{FirstName} {LastName}";
    public required SystemUserStatus Status { get; set; }
    public DateTime CreatedAt { get; set;  }
    public DateTime? LastLoginAt { get; private set; }
    public DateTime? DeactivatedAt { get; private set; }
    
    public void Activate()
    {
        if (Status == SystemUserStatus.Inactive)
        {
            Status = SystemUserStatus.Active;
        }
    }

    public void Deactivate(DateTime now)
    {
        if (Status == SystemUserStatus.Active)
        {
            Status = SystemUserStatus.Inactive;
            DeactivatedAt = now;
        }  
    }
}