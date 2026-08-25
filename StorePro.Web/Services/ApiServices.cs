using Microsoft.AspNetCore.Components.Forms;
using StorePro.Web.Models;
namespace StorePro.Web.Services;

public interface IProductService
{
    Task<PagedResult<ProductDto>> GetProductsAsync(string? search = null, int? categoryId = null,
        string? status = null, int page = 1, int pageSize = 10);
    Task<ProductDto?> GetProductAsync(int id);
    Task<ProductStatsDto?> GetStatsAsync();
    Task<ProductDto?> CreateAsync(CreateProductRequest request);
    Task<bool> UpdateAsync(int id, UpdateProductRequest request);
    Task<bool> DeleteAsync(int id);
    Task<string?> UploadImageAsync(int id, IBrowserFile file, long maxFileSize = 5 * 1024 * 1024);
}

public class ProductService(ApiClientBase api) : IProductService
{
    private readonly ApiClientBase _api = api;

    public Task<PagedResult<ProductDto>> GetProductsAsync(string? search = null, int? categoryId = null,
        string? status = null, int page = 1, int pageSize = 10)
    {
        var query = BuildQuery(("search", search), ("categoryId", categoryId?.ToString()),
            ("status", status), ("page", page.ToString()), ("pageSize", pageSize.ToString()));
        return _api.GetAsync<PagedResult<ProductDto>>($"api/products{query}")!;
    }

    public Task<ProductDto?> GetProductAsync(int id) => _api.GetAsync<ProductDto>($"api/products/{id}");

    public Task<ProductStatsDto?> GetStatsAsync() => _api.GetAsync<ProductStatsDto>("api/products/stats");

    public Task<ProductDto?> CreateAsync(CreateProductRequest request) => _api.PostAsync<ProductDto>("api/products", request);

    public Task<bool> UpdateAsync(int id, UpdateProductRequest request) => Wrap(_api.PutAsync<object>($"api/products/{id}", request));

    public Task<bool> DeleteAsync(int id) => Wrap(_api.DeleteAsync($"api/products/{id}"));

    public async Task<string?> UploadImageAsync(int id, IBrowserFile file, long maxFileSize = 5 * 1024 * 1024)
    {
        var content = new MultipartFormDataContent();
        var stream = file.OpenReadStream(maxFileSize);
        var fileContent = new StreamContent(stream);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
        content.Add(fileContent, "file", file.Name);
        var result = await _api.PostMultipartAsync<ImageResponse>($"api/products/{id}/image", content);
        return result?.ImageUrl;
    }

    private static async Task<bool> Wrap(Task task)
    {
        await task;
        return true;
    }

    private record ImageResponse(string ImageUrl);

    private static string BuildQuery(params (string Key, string? Value)[] parameters)
    {
        var pairs = parameters.Where(p => !string.IsNullOrWhiteSpace(p.Value))
                              .Select(p => $"{p.Key}={Uri.EscapeDataString(p.Value!)}");
        var query = string.Join("&", pairs);
        return string.IsNullOrEmpty(query) ? string.Empty : $"?{query}";
    }
}

public interface ICategoryService
{
    Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync();
    Task<CategoryDto?> GetCategoryAsync(int id);
    Task<CategoryDto?> CreateAsync(CreateCategoryRequest request);
    Task<bool> UpdateAsync(int id, UpdateCategoryRequest request);
    Task<bool> DeleteAsync(int id);
}

public class CategoryService(ApiClientBase api) : ICategoryService
{
    public Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync() => api.GetAsync<IReadOnlyList<CategoryDto>>("api/categories")!;

    public Task<CategoryDto?> GetCategoryAsync(int id) => api.GetAsync<CategoryDto>($"api/categories/{id}");

    public Task<CategoryDto?> CreateAsync(CreateCategoryRequest request) => api.PostAsync<CategoryDto>("api/categories", request);

    public async Task<bool> UpdateAsync(int id, UpdateCategoryRequest request)
    {
        await api.PutAsync<object>($"api/categories/{id}", request);
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        await api.DeleteAsync($"api/categories/{id}");
        return true;
    }
}

public interface IUserService
{
    Task<PagedResult<UserDetailDto>> GetUsersAsync(string? search = null, string? role = null,
        string? status = null, int page = 1, int pageSize = 10);
    Task<UserDetailDto?> GetUserAsync(int id);
    Task<UserStatsDto?> GetStatsAsync();
    Task<UserDetailDto?> CreateAsync(CreateUserRequest request);
    Task<bool> UpdateAsync(int id, UpdateUserRequest request);
    Task<bool> UpdateStatusAsync(int id, string status);
    Task<bool> DeleteAsync(int id);
}

public class UserService(ApiClientBase api) : IUserService
{
    public Task<PagedResult<UserDetailDto>> GetUsersAsync(string? search = null, string? role = null,
        string? status = null, int page = 1, int pageSize = 10)
    {
        var query = BuildQuery(("search", search), ("role", role), ("status", status),
            ("page", page.ToString()), ("pageSize", pageSize.ToString()));
        return api.GetAsync<PagedResult<UserDetailDto>>($"api/users{query}")!;
    }

    public Task<UserDetailDto?> GetUserAsync(int id) => api.GetAsync<UserDetailDto>($"api/users/{id}");

    public Task<UserStatsDto?> GetStatsAsync() => api.GetAsync<UserStatsDto>("api/users/stats");

    public Task<UserDetailDto?> CreateAsync(CreateUserRequest request) => api.PostAsync<UserDetailDto>("api/users", request);

    public async Task<bool> UpdateAsync(int id, UpdateUserRequest request)
    {
        await api.PutAsync<object>($"api/users/{id}", request);
        return true;
    }

    public async Task<bool> UpdateStatusAsync(int id, string status)
    {
        await api.PatchAsync($"api/users/{id}/status", new UpdateUserStatusRequest(status));
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        await api.DeleteAsync($"api/users/{id}");
        return true;
    }

    private record UpdateUserStatusRequest(string Status);

    private static string BuildQuery(params (string Key, string? Value)[] parameters)
    {
        var pairs = parameters.Where(p => !string.IsNullOrWhiteSpace(p.Value))
                              .Select(p => $"{p.Key}={Uri.EscapeDataString(p.Value!)}");
        var query = string.Join("&", pairs);
        return string.IsNullOrEmpty(query) ? string.Empty : $"?{query}";
    }
}

public interface IDashboardService
{
    Task<DashboardStatsDto?> GetStatsAsync();
}

public class DashboardService(ApiClientBase api) : IDashboardService
{
    public Task<DashboardStatsDto?> GetStatsAsync() => api.GetAsync<DashboardStatsDto>("api/dashboard/stats");
}
