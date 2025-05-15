using System.Collections;
using System.Data;
using System.Data.Common;

namespace RefactoringChallenge.Tests.TestHelpers;

/// <summary>
/// Enhanced mock database data reader for testing
/// </summary>
public class EnhancedMockDbDataReader : DbDataReader
{
    private readonly List<Dictionary<string, object>> _rows = new();
    private int _currentRow = -1;
    private Dictionary<string, object> _currentRowData = new();

    public override int Depth => 0;
    public override int FieldCount => _currentRowData.Count;
    public override bool HasRows => _rows.Count > 0;
    public override bool IsClosed { get; } = false;
    public override int RecordsAffected => -1;
    public override object this[int ordinal] => GetValue(ordinal);
    public override object this[string name] => GetValue(GetOrdinal(name));

    public void SetupReadResults(int rowCount)
    {
        _rows.Clear();
        for (int i = 0; i < rowCount; i++)
        {
            _rows.Add(new Dictionary<string, object>());
        }
    }

    public void SetupValue(string columnName, object value, int rowIndex = 0)
    {
        if (rowIndex >= 0 && rowIndex < _rows.Count)
        {
            _rows[rowIndex][columnName] = value;
        }
    }

    public override bool Read()
    {
        _currentRow++;
        if (_currentRow < _rows.Count)
        {
            _currentRowData = _rows[_currentRow];
            return true;
        }
        return false;
    }

    public override bool GetBoolean(int ordinal)
    {
        return Convert.ToBoolean(GetValue(ordinal));
    }

    public override byte GetByte(int ordinal)
    {
        return Convert.ToByte(GetValue(ordinal));
    }

    public override long GetBytes(int ordinal, long dataOffset, byte[]? buffer, int bufferOffset, int length)
    {
        throw new NotImplementedException();
    }

    public override char GetChar(int ordinal)
    {
        return Convert.ToChar(GetValue(ordinal));
    }

    public override long GetChars(int ordinal, long dataOffset, char[]? buffer, int bufferOffset, int length)
    {
        throw new NotImplementedException();
    }

    public override string GetDataTypeName(int ordinal)
    {
        return GetValue(ordinal).GetType().Name;
    }

    public override DateTime GetDateTime(int ordinal)
    {
        return Convert.ToDateTime(GetValue(ordinal));
    }

    public override decimal GetDecimal(int ordinal)
    {
        return Convert.ToDecimal(GetValue(ordinal));
    }

    public override double GetDouble(int ordinal)
    {
        return Convert.ToDouble(GetValue(ordinal));
    }

    public override IEnumerator GetEnumerator()
    {
        if (_rows.Count > 0)
        {
            return ((IEnumerable)_rows).GetEnumerator();
        }
        return new List<object>().GetEnumerator();
    }

    public override Type GetFieldType(int ordinal)
    {
        return GetValue(ordinal).GetType();
    }

    public override float GetFloat(int ordinal)
    {
        return Convert.ToSingle(GetValue(ordinal));
    }

    public override Guid GetGuid(int ordinal)
    {
        return (Guid)GetValue(ordinal);
    }

    public override short GetInt16(int ordinal)
    {
        return Convert.ToInt16(GetValue(ordinal));
    }

    public override int GetInt32(int ordinal)
    {
        return Convert.ToInt32(GetValue(ordinal));
    }

    public override long GetInt64(int ordinal)
    {
        return Convert.ToInt64(GetValue(ordinal));
    }

    public override string GetName(int ordinal)
    {
        return _currentRowData.Keys.ElementAt(ordinal);
    }

    public override int GetOrdinal(string name)
    {
        int i = 0;
        foreach (var key in _currentRowData.Keys)
        {
            if (key.Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
            i++;
        }
        throw new IndexOutOfRangeException($"Column '{name}' not found.");
    }

    public override string GetString(int ordinal)
    {
        return Convert.ToString(GetValue(ordinal)) ?? string.Empty;
    }

    public override object GetValue(int ordinal)
    {
        if (_currentRow < 0 || _currentRow >= _rows.Count)
        {
            throw new IndexOutOfRangeException("No current row.");
        }

        if (ordinal < 0 || ordinal >= _currentRowData.Count)
        {
            throw new IndexOutOfRangeException($"Column index {ordinal} out of range.");
        }

        return _currentRowData.Values.ElementAt(ordinal);
    }

    public override int GetValues(object[] values)
    {
        int count = Math.Min(values.Length, _currentRowData.Count);
        for (int i = 0; i < count; i++)
        {
            values[i] = GetValue(i);
        }
        return count;
    }

    public override bool IsDBNull(int ordinal)
    {
        return GetValue(ordinal) == DBNull.Value;
    }

    public override bool NextResult()
    {
        return false;
    }

    public override void Close()
    {
        // No-op in mock
    }

    public override Task<bool> ReadAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Read());
    }
}

/// <summary>
/// Enhanced mock database command for testing
/// </summary>
public class EnhancedMockDbCommand : DbCommand
{
    public override string CommandText { get; set; } = string.Empty;
    public override int CommandTimeout { get; set; }
    public override CommandType CommandType { get; set; }
    public override bool DesignTimeVisible { get; set; }
    public override UpdateRowSource UpdatedRowSource { get; set; }
    protected override DbConnection? DbConnection { get; set; }
    protected override DbParameterCollection DbParameterCollection { get; } = new EnhancedMockParameterCollection();
    protected override DbTransaction? DbTransaction { get; set; }

    // Properties needed for unit tests
    public EnhancedMockDbDataReader? ReaderToReturn { get; set; }
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

    protected override DbParameter CreateDbParameter() => new EnhancedMockParameter();

    protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
    {
        if (ExceptionToThrow != null)
            throw ExceptionToThrow;

        return ReaderToReturn ?? new EnhancedMockDbDataReader();
    }
}

/// <summary>
/// Enhanced mock parameter collection for testing
/// </summary>
public class EnhancedMockParameterCollection : DbParameterCollection
{
    private readonly List<DbParameter> _parameters = new();

    public override int Count => _parameters.Count;
    public override bool IsFixedSize => false;
    public override bool IsReadOnly => false;
    public override bool IsSynchronized => false;
    public override object SyncRoot => this;

    public override int Add(object value)
    {
        if (value is DbParameter parameter)
        {
            _parameters.Add(parameter);
            return _parameters.Count - 1;
        }
        throw new InvalidCastException("Value must be a DbParameter");
    }

    public override void AddRange(Array values)
    {
        foreach (var value in values)
        {
            Add(value);
        }
    }

    public override void Clear()
    {
        _parameters.Clear();
    }

    public override bool Contains(object value)
    {
        if (value is DbParameter parameter)
        {
            return _parameters.Contains(parameter);
        }
        return false;
    }

    public override bool Contains(string value)
    {
        return _parameters.Any(p => p.ParameterName == value);
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

    public override int IndexOf(object value)
    {
        if (value is DbParameter parameter)
        {
            return _parameters.IndexOf(parameter);
        }
        return -1;
    }

    public override int IndexOf(string parameterName)
    {
        return _parameters.FindIndex(p => p.ParameterName == parameterName);
    }

    public override void Insert(int index, object value)
    {
        if (value is DbParameter parameter)
        {
            _parameters.Insert(index, parameter);
        }
        else
        {
            throw new InvalidCastException("Value must be a DbParameter");
        }
    }

    public override void Remove(object value)
    {
        if (value is DbParameter parameter)
        {
            _parameters.Remove(parameter);
        }
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
            _parameters.RemoveAt(index);
        }
    }

    protected override DbParameter GetParameter(int index)
    {
        return _parameters[index];
    }

    protected override DbParameter GetParameter(string parameterName)
    {
        int index = IndexOf(parameterName);
        if (index >= 0)
        {
            return _parameters[index];
        }
        throw new IndexOutOfRangeException($"Parameter '{parameterName}' not found.");
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
            throw new IndexOutOfRangeException($"Parameter '{parameterName}' not found.");
        }
    }
}

/// <summary>
/// Enhanced mock parameter for testing
/// </summary>
public class EnhancedMockParameter : DbParameter
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

    public override void ResetDbType() { }
}
