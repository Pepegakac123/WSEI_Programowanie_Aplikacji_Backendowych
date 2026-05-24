namespace AppCore.Authorization;

public enum AppPolicies
{
    AdminOnly,
    ActiveUser,
    StaffOnly
}

public static class AppPoliciesExtensions
{
    public static string Name(this AppPolicies policy) => policy.ToString();
}