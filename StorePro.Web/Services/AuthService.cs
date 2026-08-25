using StorePro.Web.Models;
using System.Net.Http.Json;

namespace StorePro.Web.Services;

public interface IAuthService
{
    Task<bool> LoginAsync(string email, string password);
    Task<bool> RegisterAsync(string fullName, string email, string password);
    Task LogoutAsync();
    Task<UserDto?> GetCurrentUserAsync();
}

public class AuthService(HttpClient http, AuthStateService authState) : IAuthService
{
    public async Task<bool> LoginAsync(string email, string password)
    {
        var response = await authPost("api/auth/login", new LoginRequest(email, password));
        if (response is null) return false;
        await authState.SetAsync(response.Token, response.User);
        return true;
    }

    public async Task<bool> RegisterAsync(string fullName, string email, string password)
    {
        var response = await authPost("api/auth/register", new RegisterRequest(fullName, email, password));
        if (response is null) return false;
        await authState.SetAsync(response.Token, response.User);
        return true;
    }

    public async Task LogoutAsync() => await authState.ClearAsync();

    public async Task<UserDto?> GetCurrentUserAsync()
    {
        await Task.CompletedTask;
        return authState.CurrentUser;
    }

    private async Task<AuthResponse?> authPost(string url, object payload)
    {
        var response = await http.PostAsJsonAsync(url, payload, Json.Options);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<AuthResponse>(Json.Options);
    }
}
