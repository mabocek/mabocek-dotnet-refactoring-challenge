using System.Data;
using System.Data.Common;
using RefactoringChallenge.Factories;

namespace RefactoringChallenge.Tests.TestHelpers;

/// <summary>
/// A singleton factory that provides a shared in-memory connection for testing.
/// </summary>
public class SharedInMemoryConnectionFactory : IDatabaseConnectionFactory
{
    private static readonly Lazy<SharedInMemoryConnectionFactory> _instance =
        new Lazy<SharedInMemoryConnectionFactory>(() => new SharedInMemoryConnectionFactory());

    public static SharedInMemoryConnectionFactory Instance => _instance.Value;

    // Private constructor to prevent direct instantiation
    private SharedInMemoryConnectionFactory() { }

    public Task<DbConnection> CreateConnectionAsync()
    {
        var connection = new InMemoryDbConnection();
        connection.Open();
        return Task.FromResult<DbConnection>(connection);
    }

    public void Reset() { }

    public void InitializeDatabase() { }

    public void SeedTestData() { }

    public void ClearTestData() { }
}

/// <summary>
/// Mock database command for testing
/// </summary>
public class MockDbCommand : DbCommand
{
    public override string CommandText { get; set; } = string.Empty;
    public override int CommandTimeout { get; set; }
    public override CommandType CommandType { get; set; }
    public override bool DesignTimeVisible { get; set; }
    public override UpdateRowSource UpdatedRowSource { get; set; }
    protected override DbConnection? DbConnection { get; set; }
    protected override DbParameterCollection DbParameterCollection { get; } = new MockParameterCollection();
    protected override DbTransaction? DbTransaction { get; set; }

    public override void Cancel() { }

    public override int ExecuteNonQuery() => 1;

    public override object? ExecuteScalar() => 1;

    public override void Prepare() { }

    protected override DbParameter CreateDbParameter() => new MockParameter();

    protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        => new MockDataReader();
}

/// <summary>
/// Mock parameter collection for testing
/// </summary>
public class MockParameterCollection : DbParameterCollection
{
    private readonly List<DbParameter> _parameters = new List<DbParameter>();

    public override int Count => _parameters.Count;
    public override object SyncRoot => this;

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

    public override void Clear() => _parameters.Clear();

    public override bool Contains(string value) => _parameters.Any(p => p.ParameterName == value);

    public override bool Contains(object value) => _parameters.Contains((DbParameter)value);

    public override void CopyTo(Array array, int index)
    {
        for (int i = 0; i < _parameters.Count; i++)
        {
            array.SetValue(_parameters[i], index + i);
        }
    }

    public override System.Collections.IEnumerator GetEnumerator() => _parameters.GetEnumerator();

    public override int IndexOf(string parameterName) => _parameters.FindIndex(p => p.ParameterName == parameterName);

    public override int IndexOf(object value) => _parameters.IndexOf((DbParameter)value);

    public override void Insert(int index, object value) => _parameters.Insert(index, (DbParameter)value);

    public override void Remove(object value) => _parameters.Remove((DbParameter)value);

    public override void RemoveAt(int index) => _parameters.RemoveAt(index);

    public override void RemoveAt(string parameterName)
    {
        int index = IndexOf(parameterName);
        if (index >= 0)
        {
            _parameters.RemoveAt(index);
        }
    }

    protected override DbParameter GetParameter(int index) => _parameters[index];

    protected override DbParameter GetParameter(string parameterName)
        => _parameters.FirstOrDefault(p => p.ParameterName == parameterName)
           ?? throw new ArgumentException($"Parameter {parameterName} not found");

    protected override void SetParameter(int index, DbParameter value) => _parameters[index] = value;

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
/// Mock parameter for testing
/// </summary>
public class MockParameter : DbParameter
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

    public override void ResetDbType() => DbType = DbType.String;
}

/// <summary>
/// Mock data reader for testing
/// </summary>
public class MockDataReader : DbDataReader
{
    private bool _isClosed;

    public override bool IsClosed => _isClosed;
    public override int FieldCount => 0;
    public override int RecordsAffected => 0;
    public override bool HasRows => false;
    public override int Depth => 0;

    public override object this[int ordinal] => GetValue(ordinal);
    public override object this[string name] => GetValue(GetOrdinal(name));

    public override bool Read() => false;

    public override bool NextResult() => false;

    public override void Close() => _isClosed = true;

    public override bool GetBoolean(int ordinal) => false;
    public override byte GetByte(int ordinal) => 0;
    public override long GetBytes(int ordinal, long dataOffset, byte[]? buffer, int bufferOffset, int length) => 0;
    public override char GetChar(int ordinal) => '\0';
    public override long GetChars(int ordinal, long dataOffset, char[]? buffer, int bufferOffset, int length) => 0;
    public override string GetDataTypeName(int ordinal) => "String";
    public override DateTime GetDateTime(int ordinal) => DateTime.Now;
    public override decimal GetDecimal(int ordinal) => 0;
    public override double GetDouble(int ordinal) => 0;
    public override Type GetFieldType(int ordinal) => typeof(string);
    public override float GetFloat(int ordinal) => 0;
    public override Guid GetGuid(int ordinal) => Guid.Empty;
    public override short GetInt16(int ordinal) => 0;
    public override int GetInt32(int ordinal) => 0;
    public override long GetInt64(int ordinal) => 0;
    public override string GetName(int ordinal) => $"Column{ordinal}";
    public override int GetOrdinal(string name) => 0;
    public override string GetString(int ordinal) => string.Empty;
    public override object GetValue(int ordinal) => string.Empty;
    public override int GetValues(object[] values) => 0;
    public override bool IsDBNull(int ordinal) => true;
    public override IEnumerator<DbDataRecord> GetEnumerator() => Enumerable.Empty<DbDataRecord>().GetEnumerator();
}
