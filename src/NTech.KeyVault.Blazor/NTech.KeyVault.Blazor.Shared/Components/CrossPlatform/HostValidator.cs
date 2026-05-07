namespace NTech.KeyVault.Blazor.Shared.Components.CrossPlatform
{
    public static class HostValidator
    {
        public static HostValidationResult Validate(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return Invalid();

            input = input.Trim();

            string? scheme = null;
            string hostPort = input;

            // 1) Detect scheme
            if (input.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
            {
                scheme = "http";
                hostPort = input[7..];
            }
            else if (input.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                scheme = "https";
                hostPort = input[8..];
            }

            // 2) IPv6 with brackets: [::1]:5000
            if (hostPort.StartsWith("["))
            {
                int endBracket = hostPort.IndexOf(']');
                if (endBracket < 0)
                    return Invalid();

                string ipv6 = hostPort.Substring(1, endBracket - 1);
                string? portPart = null;

                if (endBracket + 1 < hostPort.Length)
                {
                    if (hostPort[endBracket + 1] != ':')
                        return Invalid();

                    portPart = hostPort[(endBracket + 2)..];
                }

                if (!System.Net.IPAddress.TryParse(ipv6, out var ip) ||
                    ip.AddressFamily != System.Net.Sockets.AddressFamily.InterNetworkV6)
                    return Invalid();

                int? port = null;
                if (portPart != null)
                {
                    if (!int.TryParse(portPart, out var parsedPort) || parsedPort is < 1 or > 65535)
                        return Invalid();
                    port = parsedPort;
                }

                return Valid(scheme, ipv6, port, isIp: true, isIpv6: true, isLocalhost: ipv6 == "::1");
            }

            // 3) Split host:port for IPv4 or DNS
            string host = hostPort;
            int? portNumber = null;

            var parts = hostPort.Split(':');
            if (parts.Length > 2)
                return Invalid();

            if (parts.Length == 2)
            {
                host = parts[0];

                if (!int.TryParse(parts[1], out var parsedPort) || parsedPort is < 1 or > 65535)
                    return Invalid();

                portNumber = parsedPort;
            }

            // 4) Localhost
            if (host.Equals("localhost", StringComparison.OrdinalIgnoreCase))
                return Valid(scheme, host, portNumber, isIp: false, isIpv6: false, isLocalhost: true);

            // 5) IPv4
            if (System.Net.IPAddress.TryParse(host, out var ip4) &&
                ip4.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                return Valid(scheme, host, portNumber, isIp: true, isIpv6: false, isLocalhost: host == "127.0.0.1");
            }

            // 6) DNS
            var type = Uri.CheckHostName(host);
            if (type == UriHostNameType.Dns)
            {
                if (host.StartsWith('-') || host.EndsWith('-'))
                    return Invalid();

                if (host.Contains("://"))
                    return Invalid();

                return Valid(scheme, host, portNumber, isIp: false, isIpv6: false, isLocalhost: false);
            }

            return Invalid();

            // Helpers
            static HostValidationResult Invalid() =>
                new(false, null, null, null, false, false, false, null);

            static HostValidationResult Valid(string? scheme, string host, int? port, bool isIp, bool isIpv6, bool isLocalhost)
            {
                string full;

                // IPv6 must be wrapped in brackets
                string formattedHost = isIpv6 ? $"[{host}]" : host;

                if (scheme != null)
                {
                    full = port != null
                        ? $"{scheme}://{formattedHost}:{port}"
                        : $"{scheme}://{formattedHost}";
                }
                else
                {
                    full = port != null
                        ? $"https://{formattedHost}:{port}"
                        : $"https://{formattedHost}";
                }

                return new(true, scheme, host, port, isIp, isIpv6, isLocalhost, full);
            }
        }
    }
}
