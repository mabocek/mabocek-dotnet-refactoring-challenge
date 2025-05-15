using System.Collections;
using System.Data;
using System.Data.Common;

namespace RefactoringChallenge.Tests.Unit.Factories;

/// <summary>
/// A mockable DbConnection that exposes virtual methods we can override.
/// </summary>
public class MockDbConnection : DbConnection
{
    // Using nullable string to match base class nullability
    public override string? ConnectionString { get; set; } = string.Empty;
    public override string Database => "MockDb";
    public override string DataSource => "MockSource";
    public override string ServerVersion => "1.0";
    public override ConnectionState State => ConnectionState.Open;

    public DbCommand? CommandToReturn { get; set; }

    protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
    {
        return new MockDbTransaction(this, isolationLevel);
    }

    public override void ChangeDatabase(string databaseName)
    {
    }

    public override void Close()
    {
    }

    protected override DbCommand CreateDbCommand()
    {
        return CommandToReturn ?? new EnhancedMockDbCommand();
    }

    public override void Open()
    {
    }
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

    // Properties needed for unit tests
    public DbDataReader? ReaderToReturn { get; set; }
    public Exception? ExceptionToThrow { get; set; }
    public int NonQueryResult { get; set; } = 1;
    public object? ScalarResult { get; set; } = 1;

    public override void Cancel() { }

    public override int ExecuteNonQuery()
    {
        if (ExceptionToThrow != null)
            throw ExceptionToThrow;
        return NonQueryResult;
    }

    public override object? ExecuteScalar()
    {
        if (ExceptionToThrow != null)
            throw ExceptionToThrow;
        return ScalarResult;
    }

    public override void Prepare() { }

    protected override DbParameter CreateDbParameter() => new MockParameter();

    protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
    {
        if (ExceptionToThrow != null)
            throw ExceptionToThrow;
        return ReaderToReturn ?? new MockDbDataReader();
    }
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
        foreach (DbParameter parameter in values)
        {
            _parameters.Add(parameter);
        }
    }

    public override void Clear() => _parameters.Clear();

    public override bool Contains(string value) => IndexOf(value) != -1;

    public override bool Contains(object value) => _parameters.Contains((DbParameter)value);

    public override void CopyTo(Array array, int index)
    {
        for (int i = 0; i < _parameters.Count; i++)
        {
            array.SetValue(_parameters[i], index + i);
        }
    }

    public override IEnumerator GetEnumerator() => ((IEnumerable)_parameters).GetEnumerator();

    public override int IndexOf(string parameterName)
    {
        for (int i = 0; i < _parameters.Count; i++)
        {
            if (_parameters[i].ParameterName == parameterName)
            {
                return i;
            }
        }
        return -1;
    }

    public override int IndexOf(object value) => _parameters.IndexOf((DbParameter)value);

    public override void Insert(int index, object value) => _parameters.Insert(index, (DbParameter)value);

    public override void Remove(object value) => _parameters.Remove((DbParameter)value);

    public override void RemoveAt(string parameterName) => RemoveAt(IndexOf(parameterName));

    public override void RemoveAt(int index) => _parameters.RemoveAt(index);

    protected override DbParameter GetParameter(string parameterName) => _parameters[IndexOf(parameterName)];

    protected override DbParameter GetParameter(int index) => _parameters[index];

    protected override void SetParameter(string parameterName, DbParameter value)
    {
        int index = IndexOf(parameterName);
        if (index >= 0)
        {
            _parameters[index] = value;
        }
    }

    protected override void SetParameter(int index, DbParameter value) => _parameters[index] = value;
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
    public override string SourceColumn { get; set; } = string.Empty;
    public override object? Value { get; set; }
    public override bool SourceColumnNullMapping { get; set; }
    public override int Size { get; set; }

    public override void ResetDbType() { }
}

/// <summary>
/// Mock transaction for testing
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

    public override void Commit() { }

    public override void Rollback() { }
}

/// <summary>
/// Mock data reader for testing
/// </summary>
public class MockDbDataReader : DbDataReader
{
    private bool _isClosed;
    private int _rowIndex = -1;
    private readonly List<Dictionary<string, object>> _rows = new();
    private readonly Dictionary<string, int> _columnOrdinals = new();
    private int _resultSetIndex = 0;
    private readonly List<bool> _readResults = new();
    private readonly Dictionary<string, object> _valuesByColumnName = new();
    private readonly Dictionary<int, object> _valuesByOrdinal = new();

    public override int FieldCount => 1;
    public override bool HasRows => true;
    public override bool IsClosed => _isClosed;
    public override int RecordsAffected => 1;
    public override int Depth => 0;
    public override object this[string name] => this[0];
    public override object this[int ordinal] => "Test";

    // Method to set up values that will be returned by Read()
    public void SetupReadResults(params bool[] results)
    {
        _readResults.Clear();
        _readResults.AddRange(results);
    }

    // Method to set up a value for a specific column by name
    public void SetupValue(string columnName, object value)
    {
        _valuesByColumnName[columnName] = value;
        if (!_columnOrdinals.ContainsKey(columnName))
        {
            _columnOrdinals[columnName] = _columnOrdinals.Count;
        }
    }

    // Method to set up a value for a specific column by ordinal
    public void SetupValue(int ordinal, object value)
    {
        _valuesByOrdinal[ordinal] = value;
    }

    public override bool Read()
    {
        if (_readResults.Count > 0 && _resultSetIndex < _readResults.Count)
        {
            bool result = _readResults[_resultSetIndex++];
            if (result)
            {
                _rowIndex++;
            }
            return result;
        }

        return true;
    }

    public override bool NextResult() => false;

    public override void Close() => _isClosed = true;

    public override bool GetBoolean(int ordinal) => _valuesByOrdinal.ContainsKey(ordinal) ? Convert.ToBoolean(_valuesByOrdinal[ordinal]) : false;
    public override byte GetByte(int ordinal) => _valuesByOrdinal.ContainsKey(ordinal) ? Convert.ToByte(_valuesByOrdinal[ordinal]) : (byte)0;
    public override long GetBytes(int ordinal, long dataOffset, byte[]? buffer, int bufferOffset, int length) => 0;
    public override char GetChar(int ordinal) => _valuesByOrdinal.ContainsKey(ordinal) ? Convert.ToChar(_valuesByOrdinal[ordinal]) : '\0';
    public override long GetChars(int ordinal, long dataOffset, char[]? buffer, int bufferOffset, int length) => 0;
    public override string GetDataTypeName(int ordinal) => "String";
    public override DateTime GetDateTime(int ordinal) => _valuesByOrdinal.ContainsKey(ordinal) ? Convert.ToDateTime(_valuesByOrdinal[ordinal]) : DateTime.Now;
    public override decimal GetDecimal(int ordinal)
    {
        if (_valuesByOrdinal.ContainsKey(ordinal))
        {
            var value = _valuesByOrdinal[ordinal];
            if (value == null || value == DBNull.Value)
                return 0;
            return Convert.ToDecimal(value);
        }
        return 0;
    }
    public override double GetDouble(int ordinal) => _valuesByOrdinal.ContainsKey(ordinal) ? Convert.ToDouble(_valuesByOrdinal[ordinal]) : 0;
    public override Type GetFieldType(int ordinal) => typeof(string);
    public override float GetFloat(int ordinal) => _valuesByOrdinal.ContainsKey(ordinal) ? Convert.ToSingle(_valuesByOrdinal[ordinal]) : 0;
    public override Guid GetGuid(int ordinal) => _valuesByOrdinal.ContainsKey(ordinal) ? (Guid)_valuesByOrdinal[ordinal] : Guid.Empty;
    public override short GetInt16(int ordinal) => _valuesByOrdinal.ContainsKey(ordinal) ? Convert.ToInt16(_valuesByOrdinal[ordinal]) : (short)0;
    public override int GetInt32(int ordinal) => _valuesByOrdinal.ContainsKey(ordinal) ? Convert.ToInt32(_valuesByOrdinal[ordinal]) : 0;
    public override long GetInt64(int ordinal) => _valuesByOrdinal.ContainsKey(ordinal) ? Convert.ToInt64(_valuesByOrdinal[ordinal]) : 0;
    public override string GetName(int ordinal) => $"Column{ordinal}";
    public override int GetOrdinal(string name) => _columnOrdinals.ContainsKey(name) ? _columnOrdinals[name] : 0;
    public override string GetString(int ordinal) => _valuesByOrdinal.ContainsKey(ordinal) ? Convert.ToString(_valuesByOrdinal[ordinal]) ?? string.Empty : string.Empty;
    public override object GetValue(int ordinal) => _valuesByOrdinal.ContainsKey(ordinal) ? _valuesByOrdinal[ordinal] : string.Empty;
    public override int GetValues(object[] values) => 0;
    public override bool IsDBNull(int ordinal) => !_valuesByOrdinal.ContainsKey(ordinal);
    public override IEnumerator GetEnumerator() => new EmptyEnumerator();
}

public class EmptyEnumerator : IEnumerator
{
    public object Current => null!;
    public bool MoveNext() => false;
    public void Reset() { }
}
