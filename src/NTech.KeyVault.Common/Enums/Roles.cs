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

    public static class RoleHelper
    {
        private static readonly HashSet<string> _valid =
            Enum.GetNames<Roles>()
                .Select(n => n.ToLowerInvariant())
                .ToHashSet();

        /// <summary>
        /// Parses a collection of string values into a list of valid roles, ignoring case.
        /// </summary>
        /// <remarks>Only values that match a valid role name, ignoring case, are included in the result.
        /// Invalid or unrecognized values are ignored.</remarks>
        /// <param name="values">An enumerable collection of strings representing role names to parse.</param>
        /// <returns>A list of roles corresponding to the valid and recognized role names in the input collection. The list is
        /// empty if no valid roles are found.</returns>
        public static List<Roles> ParseMany(IEnumerable<string> values)
        {
            var result = new List<Roles>();

            foreach (var value in values)
            {
                var key = value.ToLowerInvariant();

                if (_valid.Contains(key) &&
                    Enum.TryParse<Roles>(value, ignoreCase: true, out var role))
                {
                    result.Add(role);
                }
            }

            return result;
        }
    }
}
