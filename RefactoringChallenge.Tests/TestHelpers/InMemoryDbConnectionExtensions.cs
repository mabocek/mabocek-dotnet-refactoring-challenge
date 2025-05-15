using System.Data;
using System.Data.Common;
using RefactoringChallenge.Tests.Unit.Factories;

namespace RefactoringChallenge.Tests.TestHelpers;

/// <summary>
/// Extension methods for InMemoryDbConnection to make it compatible with tests
/// </summary>
public static class InMemoryDbConnectionExtensions
{
    private static readonly Dictionary<string, DataTable> CommandResults = new Dictionary<string, DataTable>();

    /// <summary>
    /// Clear all stored commands and their results
    /// </summary>
    /// <param name="connection">The connection to clear commands for</param>
    public static void ClearCommands(this InMemoryDbConnection connection)
    {
        CommandResults.Clear();
    }

    /// <summary>
    /// Set up a command to return specific results
    /// </summary>
    /// <param name="connection">The connection to set up</param>
    /// <param name="sql">SQL command text</param>
    /// <param name="result">Data table to return for this command</param>
    public static void SetupCommand(this InMemoryDbConnection connection, string sql, DataTable result)
    {
        CommandResults[sql] = result;
    }

    /// <summary>
    /// Get result for a specific SQL command
    /// </summary>
    /// <param name="connection">The connection to get results from</param>
    /// <param name="sql">SQL command text</param>
    /// <returns>Data table result or empty table if not found</returns>
    public static DataTable GetResultForCommand(this InMemoryDbConnection connection, string sql)
    {
        return CommandResults.TryGetValue(sql, out var table) ? table : new DataTable();
    }
}
