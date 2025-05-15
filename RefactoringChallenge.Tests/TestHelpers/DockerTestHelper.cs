namespace RefactoringChallenge.Tests.TestHelpers
{
    /// <summary>
    /// Helper class to handle Docker-specific configurations
    /// </summary>
    public static class DockerTestHelper
    {
        /// <summary>
        /// Determines if we are running in a Docker container
        /// </summary>
        public static bool IsRunningInDocker => Environment.GetEnvironmentVariable("DOCKER_CONTAINER") == "true";

        /// <summary>
        /// Gets the appropriate connection string for the current environment
        /// </summary>
        public static string GetConnectionString()
        {
            if (IsRunningInDocker)
            {
                // In Docker, we use the service name "mssql" instead of localhost
                return "Server=mssql,1433;Database=refactoringchallenge;User ID=sa;Password=RCPassword1!;TrustServerCertificate=True;";
            }
            else
            {
                // For local development
                return "Server=localhost,1433;Database=refactoringchallenge;User ID=sa;Password=RCPassword1!;TrustServerCertificate=True;";
            }
        }
    }
}
