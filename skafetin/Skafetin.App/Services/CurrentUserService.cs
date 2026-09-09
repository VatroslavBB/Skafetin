using System.Net.Http.Headers;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Skafetin.Shared.DTOs;

namespace Skafetin.App.Services;

public sealed class CurrentUserService : IDisposable
{
    private const string StorageKey = "skafetin.auth.session";

    private readonly HttpClient _http;
    private readonly ProtectedLocalStorage _storage;
    private Timer? _expiryTimer;

    public CurrentUserService(HttpClient http, ProtectedLocalStorage storage)
    {
        _http = http;
        _storage = storage;
    }

    public LoggedUserDto? User { get; private set; }
    public string? AccessToken { get; private set; }
    public DateTime? ExpiresAtUtc { get; private set; }
    public bool IsInitialized { get; private set; }

    public bool IsLoggedIn => User is not null && !IsExpired;

    private bool IsExpired => !ExpiresAtUtc.HasValue || ExpiresAtUtc.Value <= DateTime.UtcNow;

    public event Action? UserChanged;

    public event Action? SessionExpiring;

    public async Task InitializeAsync()
    {
        if (IsInitialized)
            return;

        try
        {
            var stored = await _storage.GetAsync<LoginResponseDto>(StorageKey);

            if (stored.Success &&
                stored.Value is not null &&
                !string.IsNullOrWhiteSpace(stored.Value.AccessToken) &&
                stored.Value.ExpiresAtUtc > DateTime.UtcNow.AddSeconds(30))
            {
                ApplySession(stored.Value);
            }
            else
            {
                await ClearSessionAsync();
            }
        }
        catch
        {
            await ClearSessionAsync();
        }
        finally
        {
            IsInitialized = true;
            UserChanged?.Invoke();
        }
    }

    public async Task LoginAsync(LoginResponseDto response)
    {
        ApplySession(response);
        await _storage.SetAsync(StorageKey, response);
        UserChanged?.Invoke();
    }

    public async Task LogoutAsync()
    {
        await ClearSessionAsync();
        UserChanged?.Invoke();
    }

    public bool HasRole(string role) => User?.Roles.Contains(role) == true;

    public bool HasAnyRole(params string[] roles) => User?.Roles.Any(roles.Contains) == true;

    public void ExpireSession()
    {
        if (User is null)
            return;

        StopExpiryTimer();

        User = null;
        AccessToken = null;
        ExpiresAtUtc = null;
        _http.DefaultRequestHeaders.Authorization = null;

        UserChanged?.Invoke();
    }

    private void ApplySession(LoginResponseDto response)
    {
        User = response.User;
        AccessToken = response.AccessToken;
        ExpiresAtUtc = response.ExpiresAtUtc;

        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", response.AccessToken);

        ScheduleExpiry(response.ExpiresAtUtc);
    }

    private void ScheduleExpiry(DateTime expiresAtUtc)
    {
        StopExpiryTimer();

        var remaining = expiresAtUtc - DateTime.UtcNow;

        if (remaining <= TimeSpan.Zero)
        {
            ExpireSession();
            return;
        }

        _expiryTimer = new Timer(_ => SessionExpiring?.Invoke(), null, remaining, Timeout.InfiniteTimeSpan);
    }

    private void StopExpiryTimer()
    {
        _expiryTimer?.Dispose();
        _expiryTimer = null;
    }

    public void Dispose() => StopExpiryTimer();

    private async Task ClearSessionAsync()
    {
        StopExpiryTimer();

        User = null;
        AccessToken = null;
        ExpiresAtUtc = null;
        _http.DefaultRequestHeaders.Authorization = null;

        try
        {
            await _storage.DeleteAsync(StorageKey);
        }
        catch
        {

        }
    }
}
