using System.Collections.ObjectModel;

namespace NTech.KeyVault.Common.Enums
{
    public enum Roles
    {
        SystemAdmin = 1,
        Admin = 2,
        User = 3
    }
    public static class RoleHierarchy
    {
        private static readonly IReadOnlyDictionary<Roles, Roles[]> _map =
            new Dictionary<Roles, Roles[]>
            {
                { Roles.SystemAdmin, new[] { Roles.Admin, Roles.User } },
                { Roles.Admin,       new[] { Roles.User } },
                { Roles.User,        Array.Empty<Roles>() }
            }
            .ToDictionary(k => k.Key, v => v.Value)
            .AsReadOnly();

        public static IEnumerable<Roles> Expand(Roles role)
        {
            yield return role;

            if (_map.TryGetValue(role, out var implied))
            {
                foreach (var r in implied)
                    yield return r;
            }
        }
    }

    public static class RoleExtensions
    {
        public static List<Roles> ExpandRoles(this IEnumerable<Roles> roles)
        {
            var expanded = roles
                .SelectMany(r => RoleHierarchy.Expand(r))
                .Distinct()
                .ToList();

            if (!expanded.Contains(Roles.User))
                expanded.Add(Roles.User);

            return expanded;
        }
    }
}
