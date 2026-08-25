using Microsoft.JSInterop;

namespace StorePro.Web.Services;

public interface ILocalStorageService
{
    ValueTask<string?> GetAsync(string key);
    ValueTask SetAsync(string key, string value);
    ValueTask RemoveAsync(string key);
}

public class LocalStorageService(IJSRuntime jsRuntime) : ILocalStorageService
{
    public ValueTask<string?> GetAsync(string key) =>
        jsRuntime.InvokeAsync<string?>("localStorage.getItem", key);

    public ValueTask SetAsync(string key, string value) =>
        jsRuntime.InvokeVoidAsync("localStorage.setItem", key, value);

    public ValueTask RemoveAsync(string key) =>
        jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
}
