using Microsoft.AspNetCore.Components.Authorization;
using NTech.KeyVault.Blazor.Shared.Models;
using NTech.KeyVault.Common.Enums;
using System.Security.Claims;

namespace NTech.KeyVault.Blazor.Shared.Services
{
    public interface IUserContext
    {
        string EntityId { get; }
        public Task<ClientUser?> GetCurrentUserAsync();
        public ClientUser? GetCurrentUser();
    }

    public class UserContext(AuthenticationStateProvider stateProvider) : IUserContext
    {
        public string EntityId => GetCurrentUser()?.Id ?? string.Empty;

        public async Task<ClientUser?> GetCurrentUserAsync()
        {
            var authState = await stateProvider.GetAuthenticationStateAsync();
            var user = authState.User;
            if (user.Identity is not null && user.Identity.IsAuthenticated)
            {
                return new ClientUser
                {
                    Id = user.FindFirst(c => c.Type == "sub")?.Value!,
                    FullName = user.FindFirst(c => c.Type == "FullName")?.Value!,
                    Username = user.FindFirst(c => c.Type == "Username")?.Value!,
                    Roles = RoleHelper.ParseMany(
                        user.Claims
                            .Where(c => c.Type == ClaimTypes.Role)
                            .Select(c => c.Value)
                            .ToList()
                        )
                };
            }
            return null;
        }

        public ClientUser? GetCurrentUser()
        {
            return GetCurrentUserAsync().GetAwaiter().GetResult();
        }
    }
}
