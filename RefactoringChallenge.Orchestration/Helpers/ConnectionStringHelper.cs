namespace RefactoringChallenge.Orchestration.Helpers;

/// <summary>
/// Helper class for securely handling connection strings
/// </summary>
public static class ConnectionStringHelper
{
    /// <summary>
    /// Returns a masked version of a connection string for logging or error messages,
    /// hiding sensitive information like passwords.
    /// </summary>
    /// <param name="connectionString">The original connection string</param>
    /// <returns>A masked connection string with password hidden</returns>
    public static string MaskConnectionString(string connectionString)
    {
        if (string.IsNullOrEmpty(connectionString))
        {
            return "[connection string is null or empty]";
        }

        // Parse connection string into key/value pairs
        var parts = connectionString.Split(';')
            .Where(part => !string.IsNullOrWhiteSpace(part))
            .Select(part => part.Trim());

        var maskedParts = parts.Select(part =>
        {
            var keyValue = part.Split('=', 2);
            if (keyValue.Length != 2)
            {
                return part;
            }

            var key = keyValue[0].Trim();

            // Mask sensitive information
            if (key.Equals("Password", StringComparison.OrdinalIgnoreCase) ||
                key.Equals("Pwd", StringComparison.OrdinalIgnoreCase))
            {
                return $"{key}=*****";
            }

            // Optionally mask other sensitive info like User ID if needed
            if (key.Equals("User ID", StringComparison.OrdinalIgnoreCase) ||
                key.Equals("Uid", StringComparison.OrdinalIgnoreCase))
            {
                return $"{key}=***masked***";
            }

            return part;
        });

        return string.Join(";", maskedParts);
    }
}
