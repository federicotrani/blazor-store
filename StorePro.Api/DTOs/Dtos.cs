using System.ComponentModel.DataAnnotations;

namespace StorePro.Api.DTOs;

// ---------- Auth ----------
public record LoginRequest(
    [Required, EmailAddress] string Email,
    [Required] string Password);

public record RegisterRequest(
    [Required, MaxLength(150)] string FullName,
    [Required, EmailAddress, MaxLength(200)] string Email,
    [Required, MinLength(6), MaxLength(100)] string Password);

public record UserDto(
    int Id, string FullName, string Email, string Role,
    string Status, DateTime? LastActiveAt, DateTime CreatedAt);

public record AuthResponse(string Token, DateTime ExpiresAt, UserDto User);

// ---------- Common ----------
public record PagedResult<T>(IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize)
{
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}

// ---------- Categories ----------
public record CategoryDto(int Id, string Name, string? Description, bool IsActive, int ProductCount, DateTime CreatedAt);

public record CreateCategoryRequest(
    [Required, MaxLength(100)] string Name,
    [MaxLength(500)] string? Description,
    bool IsActive = true);

public record UpdateCategoryRequest(
    [Required, MaxLength(100)] string Name,
    [MaxLength(500)] string? Description,
    bool IsActive);

// ---------- Products ----------
public record ProductDto(
    int Id, string Name, string? Description, decimal Price, int Stock,
    string Status, string? ImageUrl, int CategoryId, string CategoryName, DateTime CreatedAt);

public record CreateProductRequest(
    [Required, MaxLength(200)] string Name,
    [MaxLength(2000)] string? Description,
    [Range(0, 999999)] decimal Price,
    [Range(0, int.MaxValue)] int Stock,
    [Required] string Status,
    [Range(1, int.MaxValue)] int CategoryId);

public record UpdateProductRequest(
    [Required, MaxLength(200)] string Name,
    [MaxLength(2000)] string? Description,
    [Range(0, 999999)] decimal Price,
    [Range(0, int.MaxValue)] int Stock,
    [Required] string Status,
    [Range(1, int.MaxValue)] int CategoryId);

public record ProductStatsDto(int TotalProducts, int LowStockCount, int TotalCategories, int ActiveProducts);

// ---------- Users ----------
public record UserDetailDto(
    int Id, string FullName, string Email, string Role, string Status,
    DateTime? LastActiveAt, DateTime CreatedAt);

public record CreateUserRequest(
    [Required, MaxLength(150)] string FullName,
    [Required, EmailAddress, MaxLength(200)] string Email,
    [Required, MinLength(6), MaxLength(100)] string Password,
    [Required] string Role,
    string Status = "Active");

public record UpdateUserRequest(
    [Required, MaxLength(150)] string FullName,
    [Required, EmailAddress, MaxLength(200)] string Email,
    [Required] string Role,
    [Required] string Status,
    [MinLength(6), MaxLength(100)] string? Password);

public record UpdateUserStatusRequest([Required] string Status);

public record UserStatsDto(int TotalUsers, int ActiveUsers, int AdminCount, int PendingReview);

// ---------- Dashboard ----------
public record DashboardStatsDto(
    int TotalProducts,
    int TotalCategories,
    int TotalUsers,
    int LowStockProducts,
    int ActiveUsers,
    int ActiveProducts,
    IReadOnlyList<CategorySalesDto> TopCategories,
    IReadOnlyList<ActivityPointDto> WeeklyActivity);

public record CategorySalesDto(string CategoryName, int ProductCount, decimal TotalValue);

public record ActivityPointDto(string Label, int Value);
