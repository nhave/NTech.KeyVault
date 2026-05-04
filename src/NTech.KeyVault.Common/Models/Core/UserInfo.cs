using System.Text;
using System.Text.Json;

namespace NTech.KeyVault.Common.Models.Core
{
    public class UserInfo
    {
        /// <summary>
        /// Gets or sets the full name of the person.
        /// </summary>
        public string FullName { get; set; } = default!;

        /// <summary>
        /// Gets or sets the email address associated with the user.
        /// </summary>
        public string Email { get; set; } = default!;

        /// <summary>
        /// Gets the abbreviated form of the full name.
        /// </summary>
        public string FullNameShort => GetShortName(FullName);

        // A set of common name parts to ignore when generating the short name.
        private static readonly HashSet<string> IgnoredParts = new(StringComparer.OrdinalIgnoreCase)
        {
            "van", "von", "de", "der", "af", "la", "le"
        };

        /// <summary>
        /// Generates a shortened version of a full name by including the first and last names, and abbreviating any
        /// middle names to initials.
        /// </summary>
        /// <remarks>Ignored parts are excluded from the result. If the full name contains only one part
        /// after filtering, that part is returned as is.</remarks>
        /// <param name="fullName">The full name to be shortened. Cannot be null, empty, or consist only of whitespace.</param>
        /// <returns>A string containing the shortened name, with middle names represented as initials. Returns an empty string
        /// if the input is null, empty, or contains only ignored parts.</returns>
        private static string GetShortName(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                return string.Empty;

            var parts = fullName
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Where(p => !IgnoredParts.Contains(p))
                .ToArray();

            if (parts.Length == 0)
                return string.Empty;

            if (parts.Length == 1)
                return parts[0];

            var first = parts[0];
            var last = parts[^1];

            var middleInitials = parts
                .Skip(1)
                .Take(parts.Length - 2)
                .Select(p => $"{p[0]}.");

            var middle = string.Join(" ", middleInitials);

            return string.IsNullOrEmpty(middle)
                ? $"{first} {last}"
                : $"{first} {middle} {last}";
        }

        /// <summary>
        /// Serializes the current object instance to a UTF-8 encoded JSON byte array.
        /// </summary>
        /// <remarks>The resulting byte array can be used for storage, transmission, or interoperability
        /// with systems that consume JSON data. The serialization includes all public properties of the object. If the
        /// object contains circular references or unsupported types, serialization may fail.</remarks>
        /// <returns>A byte array containing the UTF-8 encoded JSON representation of the object.</returns>
        public byte[] Serialize()
        {
            var json = JsonSerializer.Serialize(this);
            return Encoding.UTF8.GetBytes(json);
        }

        /// <summary>
        /// Deserializes a UTF-8 encoded JSON byte array into a UserInfo object.
        /// </summary>
        /// <param name="data">A byte array containing the UTF-8 encoded JSON representation of a UserInfo object. Cannot be null.</param>
        /// <returns>A UserInfo object deserialized from the specified byte array, or null if the JSON is empty or invalid.</returns>
        public static UserInfo? Deserialize(byte[] data)
        {
            var json = Encoding.UTF8.GetString(data);
            return JsonSerializer.Deserialize<UserInfo>(json);
        }
    }

}
