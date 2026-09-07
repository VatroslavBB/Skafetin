using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace Skafetin.App.Services;

public sealed class SkafetinAuthenticationStateProvider : AuthenticationStateProvider, IDisposable
{
    private const string AuthenticationType = "Skafetin";

    private static readonly AuthenticationState Anonymous =
        new(new ClaimsPrincipal(new ClaimsIdentity()));

    private readonly CurrentUserService _currentUser;

    public SkafetinAuthenticationStateProvider(CurrentUserService currentUser)
    {
        _currentUser = currentUser;
        _currentUser.UserChanged += OnUserChanged;
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync() =>
        Task.FromResult(BuildState());

    private AuthenticationState BuildState()
    {
        if (!_currentUser.IsLoggedIn || _currentUser.User is null)
            return Anonymous;

        var user = _currentUser.User;

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.DisplayName),
            new(ClaimTypes.Email, user.Email)
        };

        if (user.EmployeeId.HasValue)
            claims.Add(new Claim("employeeId", user.EmployeeId.Value.ToString()));

        if (user.LocationId.HasValue)
            claims.Add(new Claim("locationId", user.LocationId.Value.ToString()));

        claims.AddRange(user.Roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var identity = new ClaimsIdentity(claims, AuthenticationType);
        return new AuthenticationState(new ClaimsPrincipal(identity));
    }

    private void OnUserChanged() =>
        NotifyAuthenticationStateChanged(Task.FromResult(BuildState()));

    public void Dispose() => _currentUser.UserChanged -= OnUserChanged;
}
