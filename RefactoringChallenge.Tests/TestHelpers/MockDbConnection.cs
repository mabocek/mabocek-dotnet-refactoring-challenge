using System.Data;
using System.Data.Common;

namespace RefactoringChallenge.Tests.TestHelpers;

/// <summary>
/// Mock database connection for testing
/// </summary>
public class MockDbConnection : DbConnection
{
    private ConnectionState _state = ConnectionState.Closed;
    private readonly Dictionary<string, EnhancedMockDbCommand> _commandMap = new();

    public override string ConnectionString { get; set; } = "InMemory";
    public override int ConnectionTimeout => 30;
    public override string Database => "MockDb";
    public override string DataSource => "InMemory";
    public override string ServerVersion => "1.0";
    public override ConnectionState State => _state;

    public override void Open()
    {
        _state = ConnectionState.Open;
    }

    public override void Close()
    {
        _state = ConnectionState.Closed;
    }

    protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
    {
        return new MockDbTransaction(this, isolationLevel);
    }

    protected override DbCommand CreateDbCommand()
    {
        return new EnhancedMockDbCommand();
    }

    public override void ChangeDatabase(string databaseName)
    {
        // No-op for in-memory database
    }

    // Additional methods for testing
    public void SetupCommand(string sql, EnhancedMockDbDataReader reader)
    {
        var command = new EnhancedMockDbCommand
        {
            CommandText = sql,
            ReaderToReturn = reader
        };
        _commandMap[sql] = command;
    }

    public void SetupCommand(string sql, DataTable result)
    {
        var reader = new EnhancedMockDbDataReader();
        reader.SetupReadResults(result.Rows.Count);

        for (int rowIndex = 0; rowIndex < result.Rows.Count; rowIndex++)
        {
            for (int colIndex = 0; colIndex < result.Columns.Count; colIndex++)
            {
                reader.SetupValue(result.Columns[colIndex].ColumnName, result.Rows[rowIndex][colIndex], rowIndex);
            }
        }

        SetupCommand(sql, reader);
    }

    public void SetupScalarCommand(string sql, object? result)
    {
        var command = new EnhancedMockDbCommand
        {
            CommandText = sql,
            ScalarResult = result
        };
        _commandMap[sql] = command;
    }

    public void SetupNonQueryCommand(string sql, int result)
    {
        var command = new EnhancedMockDbCommand
        {
            CommandText = sql,
            NonQueryResult = result
        };
        _commandMap[sql] = command;
    }

    public void SetupExceptionCommand(string sql, Exception exception)
    {
        var command = new EnhancedMockDbCommand
        {
            CommandText = sql,
            ExceptionToThrow = exception
        };
        _commandMap[sql] = command;
    }

    public void ClearCommands()
    {
        _commandMap.Clear();
    }

    internal EnhancedMockDbCommand? GetCommand(string sql)
    {
        _commandMap.TryGetValue(sql, out var command);
        return command;
    }
}

/// <summary>
/// Mock database transaction for testing
/// </summary>
public class MockDbTransaction : DbTransaction
{
    public MockDbTransaction(DbConnection connection, IsolationLevel isolationLevel)
    {
        DbConnection = connection;
        IsolationLevel = isolationLevel;
    }

    public override IsolationLevel IsolationLevel { get; }
    protected override DbConnection DbConnection { get; }

    public override void Commit()
    {
        // No-op in mock
    }

    public override void Rollback()
    {
        // No-op in mock
    }
}
