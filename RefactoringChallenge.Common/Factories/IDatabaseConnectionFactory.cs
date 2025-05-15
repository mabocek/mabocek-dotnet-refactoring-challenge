namespace RefactoringChallenge.Factories;

/// <summary>
/// Database connection factory interface
/// </summary>
public interface IDatabaseConnectionFactory
{
    /// <summary>
    /// Creates a new database connection
    /// </summary>
    /// <returns>An open database connection</returns>
    Task<System.Data.Common.DbConnection> CreateConnectionAsync();
}
