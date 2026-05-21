namespace AppCore.Users;

public interface ISystemUser
{
    string Id { get; }
    string Email { get; }
    string FirstName { get; }
    string LastName { get; }
    string FullName { get; }
    string Department { get; }
    SystemUserStatus Status { get; }
    DateTime CreatedAt { get; }
}

public enum SystemUserStatus
{
    Active,
    Inactive,
    Locked,
    PendingActivation   
}

public enum UserRole
{
    Administrator,
    GateController,
    Driver
}