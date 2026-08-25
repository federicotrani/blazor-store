using StorePro.Web.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace StorePro.Web.Services;

/// <summary>
/// Estado de autenticación compartido: token JWT y usuario actual.
/// </summary>
public class AuthStateService(ILocalStorageService storage)
{
    public const string TokenKey = "storepro.token";
    public const string UserKey = "storepro.user";

    public string? Token { get; private set; }
    public UserDto? CurrentUser { get; private set; }
    public bool IsAuthenticated => !string.IsNullOrEmpty(Token);

    public event Action? Changed;

    public async Task InitializeAsync()
    {
        Token = await storage.GetAsync(TokenKey);
        var userJson = await storage.GetAsync(UserKey);
        if (!string.IsNullOrEmpty(userJson))
        {
            try
            {
                CurrentUser = JsonSerializer.Deserialize<UserDto>(userJson, Json.Options);
            }
            catch
            {
                await ClearAsync();
            }
        }

        // Expiración simple: si el token no decodifica bien, limpiar
        if (IsAuthenticated && CurrentUser is null)
            await ClearAsync();

        Changed?.Invoke();
    }

    public async Task SetAsync(string token, UserDto user)
    {
        Token = token;
        CurrentUser = user;
        await storage.SetAsync(TokenKey, token);
        await storage.SetAsync(UserKey, JsonSerializer.Serialize(user, Json.Options));
        Changed?.Invoke();
    }

    public async Task ClearAsync()
    {
        Token = null;
        CurrentUser = null;
        await storage.RemoveAsync(TokenKey);
        await storage.RemoveAsync(UserKey);
        Changed?.Invoke();
    }
}

/// <summary>
/// Adjunta el token JWT a cada petición hacia la API.
/// </summary>
public class AuthorizationMessageHandler(AuthStateService authState) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        await authState.InitializeAsync();
        if (!string.IsNullOrEmpty(authState.Token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authState.Token);

        return await base.SendAsync(request, cancellationToken);
    }
}

/// <summary>
/// Excepción para errores de la API con mensaje legible.
/// </summary>
public class ApiException(string message, int statusCode) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
}

public static class Json
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);
}

/// <summary>
/// Cliente base para consumir la API REST con gestión de token y errores.
/// </summary>
public class ApiClientBase(HttpClient http, AuthStateService authState)
{
    protected readonly HttpClient Http = http;
    protected readonly AuthStateService AuthState = authState;

    public async Task<T?> GetAsync<T>(string url)
    {
        var response = await Http.GetAsync(url);
        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<T>(Json.Options);
    }

    public async Task<T?> PostAsync<T>(string url, object payload)
    {
        var response = await Http.PostAsJsonAsync(url, payload, Json.Options);
        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<T>(Json.Options);
    }

    public async Task<T?> PutAsync<T>(string url, object payload)
    {
        var response = await Http.PutAsJsonAsync(url, payload, Json.Options);
        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<T>(Json.Options);
    }

    public async Task PatchAsync(string url, object payload)
    {
        var response = await Http.PatchAsJsonAsync(url, payload, Json.Options);
        await EnsureSuccessAsync(response);
    }

    public async Task DeleteAsync(string url)
    {
        var response = await Http.DeleteAsync(url);
        await EnsureSuccessAsync(response);
    }

    public async Task<T?> PostMultipartAsync<T>(string url, MultipartFormDataContent content)
    {
        var response = await Http.PostAsync(url, content);
        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<T>(Json.Options);
    }

    protected async Task<T?> GetUnauthenticatedAsync<T>(string url)
    {
        var response = await Http.GetAsync(url);
        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<T>(Json.Options);
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
            return;

        string message;
        try
        {
            var error = await response.Content.ReadFromJsonAsync<ApiError>(Json.Options);
            message = error?.Message ?? response.ReasonPhrase ?? "Error en la solicitud.";
        }
        catch
        {
            message = response.ReasonPhrase ?? "Error en la solicitud.";
        }

        throw new ApiException(message, (int)response.StatusCode);
    }
}
