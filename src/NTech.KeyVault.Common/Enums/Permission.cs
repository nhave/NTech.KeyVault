using System.Security.AccessControl;

namespace NTech.KeyVault.Common.Enums
{
    public enum Permission
    {
        Application_Admin,
        Application_Config_Read,
        Application_Config_Write,
        Application_Secret_Read,
        Application_Secret_Write,
        Team_UserManagement,
        Team_Settings_Read
    }

    public static class PermissionHelper
    {
        public static IReadOnlyDictionary<string, List<Permission>> GetGroupedPermissions()
        {
            return Enum.GetValues(typeof(Permission))
                .Cast<Permission>()
                .GroupBy(p => GetPrefix(p))
                .ToDictionary(
                    g => g.Key,
                    g => g.ToList()
                );
        }

        public static List<Permission> Parse(
            ResourceType resourceType,
            IEnumerable<string> values)
        {
            if (values is null)
                throw new ArgumentNullException(nameof(values));

            var validNames = Enum.GetNames<Permission>().ToHashSet();
            var result = new List<Permission>();

            foreach (var value in values)
            {
                if (!validNames.Contains(value))
                    throw new ArgumentException($"Invalid permission: '{value}'");

                var permission = Enum.Parse<Permission>(value);

                if (!IsValidPermissionFor(resourceType, permission))
                    throw new ArgumentException(
                        $"Permission '{value}' does not belong to resource type '{resourceType}'");

                result.Add(permission);
            }

            return result;
        }


        public static List<Permission> GetPermissionsFor(string prefix)
        {
            return Enum.GetValues(typeof(Permission))
                .Cast<Permission>()
                .Where(p => GetPrefix(p).Equals(prefix, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        public static List<Permission> GetPermissionsFor(ResourceType resourceType)
        {
            return GetPermissionsFor(resourceType.ToString());
        }

        public static List<string> GetPermissionsForAsStrings(string prefix)
        {
            return GetPermissionsFor(prefix)
                .Select(p => p.ToString())
                .ToList();
        }

        public static List<string> GetPermissionsForAsStrings(ResourceType resourceType)
        {
            return GetPermissionsFor(resourceType.ToString())
                .Select(p => p.ToString())
                .ToList();
        }

        public static string GetPrefix(Permission permission)
        {
            var name = permission.ToString();
            var index = name.IndexOf('_');

            return index > 0
                ? name.Substring(0, index)
                : name;
        }

        public static bool IsValidPermissionFor(ResourceType resourceType, Permission permission)
        {
            var prefix = PermissionHelper.GetPrefix(permission);
            return prefix.Equals(resourceType.ToString(), StringComparison.OrdinalIgnoreCase);
        }
    }
}
