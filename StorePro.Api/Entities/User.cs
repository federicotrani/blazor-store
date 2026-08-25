namespace StorePro.Api.Entities;

public class User
{
    public int Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string Role { get; set; } = UserRoles.Customer;

    public string Status { get; set; } = UserStatuses.Active;

    public DateTime? LastActiveAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public static class UserRoles
{
    public const string Admin = "Admin";
    public const string Manager = "Manager";
    public const string Customer = "Customer";

    public static readonly IReadOnlyList<string> All = [Admin, Manager, Customer];
}

public static class UserStatuses
{
    public const string Active = "Active";
    public const string Suspended = "Suspended";

    public static readonly IReadOnlyList<string> All = [Active, Suspended];
}
