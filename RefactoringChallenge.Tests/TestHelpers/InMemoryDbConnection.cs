using System.Data;
using System.Data.Common;
using RefactoringChallenge.Factories;

namespace RefactoringChallenge.Tests.TestHelpers;

/// <summary>
/// A simple in-memory database connection factory implementation
/// </summary>
public class InMemoryDbConnection : DbConnection
{
    private ConnectionState _state = ConnectionState.Closed;
    private readonly Dictionary<string, DataTable> _commandResults = new Dictionary<string, DataTable>();

    // Constructor
    public InMemoryDbConnection()
    {
        _state = ConnectionState.Open;
    }

    // DbConnection implementation
    public override string? ConnectionString { get; set; } = "InMemory";
    public override int ConnectionTimeout => 30;
    public override string Database => "InMemoryDb";
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
        return new InMemoryTransaction(this, isolationLevel);
    }

    protected override DbCommand CreateDbCommand()
    {
        return new InMemoryCommand(this);
    }

    public override void ChangeDatabase(string databaseName)
    {
        // No-op for in-memory database
    }

    // Additional methods for testing
    public void ClearCommands()
    {
        _commandResults.Clear();
    }

    public void SetupCommand(string sql, DataTable result)
    {
        _commandResults[sql] = result;
    }

    public DataTable GetResultForCommand(string sql)
    {
        return _commandResults.TryGetValue(sql, out var table) ? table : new DataTable();
    }

    public void ResetData()
    {
        // Reset any in-memory data
        ClearCommands();
    }

    public void InitializeTables()
    {
        // Initialize tables if needed
    }

    public void SeedTestData()
    {
        // Seed test data in-memory
    }
}

/// <summary>
/// In-memory transaction implementation
/// </summary>
public class InMemoryTransaction : DbTransaction
{
    public InMemoryTransaction(DbConnection connection, IsolationLevel isolationLevel)
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

/// <summary>
/// In-memory command implementation
/// </summary>
public class InMemoryCommand : DbCommand
{
    private readonly InMemoryDbConnection _connection;
    private readonly InMemoryParameterCollection _parameters = new();

    public InMemoryCommand(InMemoryDbConnection connection)
    {
        _connection = connection;
        DbParameterCollection = _parameters;
    }

    public override string CommandText { get; set; } = string.Empty;
    public override int CommandTimeout { get; set; } = 30;
    public override CommandType CommandType { get; set; } = CommandType.Text;
    public override bool DesignTimeVisible { get; set; } = true;
    public override UpdateRowSource UpdatedRowSource { get; set; } = UpdateRowSource.None;
    protected override DbConnection? DbConnection { get; set; }
    protected override DbParameterCollection DbParameterCollection { get; }
    protected override DbTransaction? DbTransaction { get; set; }

    public override void Cancel()
    {
        // No-op in mock
    }

    public override int ExecuteNonQuery()
    {
        // Return a successful result
        return 1;
    }

    public override object? ExecuteScalar()
    {
        // Return a generic value
        return 1;
    }

    public override void Prepare()
    {
        // No-op in mock
    }

    protected override DbParameter CreateDbParameter()
    {
        return new InMemoryParameter();
    }

    protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
    {
        return new InMemoryDataReader();
    }
}

/// <summary>
/// In-memory parameter collection implementation
/// </summary>
public class InMemoryParameterCollection : DbParameterCollection
{
    private readonly List<DbParameter> _parameters = new();

    public override int Count => _parameters.Count;
    public override object SyncRoot => _parameters;

    public override int Add(object value)
    {
        _parameters.Add((DbParameter)value);
        return _parameters.Count - 1;
    }

    public override void AddRange(Array values)
    {
        foreach (DbParameter param in values)
        {
            _parameters.Add(param);
        }
    }

    public override void Clear()
    {
        _parameters.Clear();
    }

    public override bool Contains(string value)
    {
        return _parameters.Any(p => p.ParameterName == value);
    }

    public override bool Contains(object value)
    {
        return _parameters.Contains((DbParameter)value);
    }

    public override void CopyTo(Array array, int index)
    {
        for (int i = 0; i < _parameters.Count; i++)
        {
            array.SetValue(_parameters[i], index + i);
        }
    }

    public override System.Collections.IEnumerator GetEnumerator()
    {
        return _parameters.GetEnumerator();
    }

    public override int IndexOf(string parameterName)
    {
        return _parameters.FindIndex(p => p.ParameterName == parameterName);
    }

    public override int IndexOf(object value)
    {
        return _parameters.IndexOf((DbParameter)value);
    }

    public override void Insert(int index, object value)
    {
        _parameters.Insert(index, (DbParameter)value);
    }

    public override void Remove(object value)
    {
        _parameters.Remove((DbParameter)value);
    }

    public override void RemoveAt(int index)
    {
        _parameters.RemoveAt(index);
    }

    public override void RemoveAt(string parameterName)
    {
        int index = IndexOf(parameterName);
        if (index >= 0)
        {
            RemoveAt(index);
        }
    }

    protected override DbParameter GetParameter(int index)
    {
        return _parameters[index];
    }

    protected override DbParameter GetParameter(string parameterName)
    {
        return _parameters.FirstOrDefault(p => p.ParameterName == parameterName) ??
               throw new ArgumentException($"Parameter {parameterName} not found");
    }

    protected override void SetParameter(int index, DbParameter value)
    {
        _parameters[index] = value;
    }

    protected override void SetParameter(string parameterName, DbParameter value)
    {
        int index = IndexOf(parameterName);
        if (index >= 0)
        {
            _parameters[index] = value;
        }
        else
        {
            throw new ArgumentException($"Parameter {parameterName} not found");
        }
    }
}

/// <summary>
/// In-memory parameter implementation
/// </summary>
public class InMemoryParameter : DbParameter
{
    public override DbType DbType { get; set; }
    public override ParameterDirection Direction { get; set; }
    public override bool IsNullable { get; set; }
    public override string ParameterName { get; set; } = string.Empty;
    public override int Size { get; set; }
    public override string SourceColumn { get; set; } = string.Empty;
    public override bool SourceColumnNullMapping { get; set; }
    public override object? Value { get; set; }
    public override DataRowVersion SourceVersion { get; set; }

    public override void ResetDbType()
    {
        DbType = DbType.String;
    }
}

/// <summary>
/// In-memory data reader implementation
/// </summary>
public class InMemoryDataReader : DbDataReader
{
    private bool _isClosed;
    private int _currentRow = -1;

    public override bool IsClosed => _isClosed;
    public override int FieldCount => 0;
    public override int RecordsAffected => 0;
    public override bool HasRows => false;
    public override int Depth => 0;

    public override bool Read()
    {
        _currentRow++;
        return false; // No rows by default
    }

    public override bool NextResult()
    {
        return false;
    }

    public override void Close()
    {
        _isClosed = true;
    }

    public override bool GetBoolean(int ordinal) => false;
    public override byte GetByte(int ordinal) => 0;
    public override long GetBytes(int ordinal, long dataOffset, byte[]? buffer, int bufferOffset, int length) => 0;
    public override char GetChar(int ordinal) => 'a';
    public override long GetChars(int ordinal, long dataOffset, char[]? buffer, int bufferOffset, int length) => 0;
    public override string GetDataTypeName(int ordinal) => "string";
    public override DateTime GetDateTime(int ordinal) => DateTime.Now;
    public override decimal GetDecimal(int ordinal) => 0;
    public override double GetDouble(int ordinal) => 0;
    public override Type GetFieldType(int ordinal) => typeof(string);
    public override float GetFloat(int ordinal) => 0;
    public override Guid GetGuid(int ordinal) => Guid.Empty;
    public override short GetInt16(int ordinal) => 0;
    public override int GetInt32(int ordinal) => 0;
    public override long GetInt64(int ordinal) => 0;
    public override string GetName(int ordinal) => string.Empty;
    public override int GetOrdinal(string name) => 0;
    public override string GetString(int ordinal) => string.Empty;

    public override object GetValue(int ordinal) => string.Empty;
    public override int GetValues(object[] values) => 0;
    public override bool IsDBNull(int ordinal) => true;

    public override object this[int ordinal] => GetValue(ordinal);
    public override object this[string name] => GetValue(GetOrdinal(name));
    public override IEnumerator<DbDataRecord> GetEnumerator() => Enumerable.Empty<DbDataRecord>().GetEnumerator();
}
