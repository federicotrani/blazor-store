namespace StorePro.Web.Models;

// ---------- Auth ----------
public record LoginRequest(string Email, string Password);

public record RegisterRequest(string FullName, string Email, string Password);

public record UserDto(
    int Id, string FullName, string Email, string Role,
    string Status, DateTime? LastActiveAt, DateTime CreatedAt);

public record AuthResponse(string Token, DateTime ExpiresAt, UserDto User);

public record ApiError(string Message);

// ---------- Common ----------
public record PagedResult<T>(IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize)
{
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}

// ---------- Categories ----------
public record CategoryDto(int Id, string Name, string? Description, bool IsActive, int ProductCount, DateTime CreatedAt);

public record CreateCategoryRequest(string Name, string? Description, bool IsActive = true);

public record UpdateCategoryRequest(string Name, string? Description, bool IsActive);

// ---------- Products ----------
public record ProductDto(
    int Id, string Name, string? Description, decimal Price, int Stock,
    string Status, string? ImageUrl, int CategoryId, string CategoryName, DateTime CreatedAt);

public record CreateProductRequest(string Name, string? Description, decimal Price, int Stock, string Status, int CategoryId);

public record UpdateProductRequest(string Name, string? Description, decimal Price, int Stock, string Status, int CategoryId);

public record ProductStatsDto(int TotalProducts, int LowStockCount, int TotalCategories, int ActiveProducts);

// ---------- Users ----------
public record UserDetailDto(
    int Id, string FullName, string Email, string Role, string Status,
    DateTime? LastActiveAt, DateTime CreatedAt);

public record CreateUserRequest(string FullName, string Email, string Password, string Role, string Status = "Active");

public record UpdateUserRequest(string FullName, string Email, string Role, string Status, string? Password);

public record UserStatsDto(int TotalUsers, int ActiveUsers, int AdminCount, int PendingReview);

// ---------- Dashboard ----------
public record DashboardStatsDto(
    int TotalProducts, int TotalCategories, int TotalUsers, int LowStockProducts,
    int ActiveUsers, int ActiveProducts,
    IReadOnlyList<CategorySalesDto> TopCategories,
    IReadOnlyList<ActivityPointDto> WeeklyActivity);

public record CategorySalesDto(string CategoryName, int ProductCount, decimal TotalValue);

public record ActivityPointDto(string Label, int Value);

public static class ProductStatuses
{
    public const string Active = "Active";
    public const string Draft = "Draft";
    public const string OutOfStock = "OutOfStock";

    public static readonly IReadOnlyList<string> All = [Active, Draft, OutOfStock];

    public static string ToSpanish(string status) => status switch
    {
        Active => "Activo",
        Draft => "Borrador",
        OutOfStock => "Agotado",
        _ => status
    };
}

public static class UserRoles
{
    public const string Admin = "Admin";
    public const string Manager = "Manager";
    public const string Customer = "Customer";

    public static readonly IReadOnlyList<string> All = [Admin, Manager, Customer];

    public static string ToSpanish(string role) => role switch
    {
        Admin => "Administrador",
        Manager => "Gestor",
        Customer => "Cliente",
        _ => role
    };
}

public static class UserStatuses
{
    public const string Active = "Active";
    public const string Suspended = "Suspended";

    public static readonly IReadOnlyList<string> All = [Active, Suspended];

    public static string ToSpanish(string status) => status switch
    {
        Active => "Activo",
        Suspended => "Suspendido",
        _ => status
    };
}
