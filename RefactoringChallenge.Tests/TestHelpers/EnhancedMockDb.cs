using System.Data;
using System.Data.Common;

namespace RefactoringChallenge.Tests.Unit.Factories;

/// <summary>
/// Enhanced mock DbCommand with additional testing functionality
/// </summary>
public class EnhancedMockDbCommand : DbCommand
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

        return ReaderToReturn ?? new EnhancedMockDbDataReader();
    }
}

/// <summary>
/// Enhanced mock DbDataReader with additional testing functionality
/// </summary>
public class EnhancedMockDbDataReader : DbDataReader
{
    private bool _isClosed;
    private int _rowIndex = -1;
    private readonly List<Dictionary<string, object>> _rows = new();
    private readonly Dictionary<string, int> _columnOrdinals = new();
    private int _resultSetIndex = 0;
    private readonly List<bool> _readResults = new();
    private readonly Dictionary<string, object> _valuesByColumnName = new();

    public override bool IsClosed => _isClosed;
    public override int FieldCount => _columnOrdinals.Count > 0 ? _columnOrdinals.Count : 1;
    public override int RecordsAffected => 0;
    public override int Depth => 0;
    public override bool HasRows => _rows.Count > 0 || _readResults.Contains(true);
    public override object this[string name] => GetValue(GetOrdinal(name));
    public override object this[int ordinal] => GetValue(ordinal);

    public void AddRow(Dictionary<string, object> row)
    {
        _rows.Add(row);
        // Update column ordinals
        foreach (var key in row.Keys)
        {
            if (!_columnOrdinals.ContainsKey(key))
            {
                _columnOrdinals[key] = _columnOrdinals.Count;
            }
        }
    }

    // Method to set up values that will be returned by Read()
    public void SetupReadResults(params bool[] results)
    {
        _readResults.Clear();
        _readResults.AddRange(results);
    }

    // Method to set up a value for a specific column
    public void SetupValue(string columnName, object value)
    {
        _valuesByColumnName[columnName] = value;
        if (!_columnOrdinals.ContainsKey(columnName))
        {
            _columnOrdinals[columnName] = _columnOrdinals.Count;
        }
    }

    // Method to set up a value for a column by ordinal position
    public void SetupValue(int ordinal, object value)
    {
        string columnName = $"Column{ordinal}";
        _valuesByColumnName[columnName] = value;
        _columnOrdinals[columnName] = ordinal;
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

        _rowIndex++;
        return _rowIndex < _rows.Count;
    }

    public override bool NextResult() => false;

    public override void Close() => _isClosed = true;

    public override bool GetBoolean(int ordinal) => Convert.ToBoolean(GetValue(ordinal));
    public override byte GetByte(int ordinal) => Convert.ToByte(GetValue(ordinal));
    public override long GetBytes(int ordinal, long dataOffset, byte[]? buffer, int bufferOffset, int length) => 0;
    public override char GetChar(int ordinal) => Convert.ToChar(GetValue(ordinal));
    public override long GetChars(int ordinal, long dataOffset, char[]? buffer, int bufferOffset, int length) => 0;
    public override string GetDataTypeName(int ordinal) => GetValue(ordinal)?.GetType().Name ?? "String";
    public override DateTime GetDateTime(int ordinal) => Convert.ToDateTime(GetValue(ordinal));
    public override decimal GetDecimal(int ordinal) => Convert.ToDecimal(GetValue(ordinal));
    public override double GetDouble(int ordinal) => Convert.ToDouble(GetValue(ordinal));
    public override Type GetFieldType(int ordinal) => GetValue(ordinal)?.GetType() ?? typeof(string);
    public override float GetFloat(int ordinal) => Convert.ToSingle(GetValue(ordinal));
    public override Guid GetGuid(int ordinal) => (Guid)GetValue(ordinal);
    public override short GetInt16(int ordinal) => Convert.ToInt16(GetValue(ordinal));
    public override int GetInt32(int ordinal) => Convert.ToInt32(GetValue(ordinal));
    public override long GetInt64(int ordinal) => Convert.ToInt64(GetValue(ordinal));

    public override string GetName(int ordinal)
    {
        return _columnOrdinals.FirstOrDefault(x => x.Value == ordinal).Key ?? $"Column{ordinal}";
    }

    public override int GetOrdinal(string name)
    {
        return _columnOrdinals.TryGetValue(name, out int ordinal) ? ordinal : 0;
    }

    public override string GetString(int ordinal) => Convert.ToString(GetValue(ordinal)) ?? string.Empty;

    public override object GetValue(int ordinal)
    {
        // First check if we're using mocked column values
        if (_rowIndex >= 0 && _rowIndex < _rows.Count)
        {
            var row = _rows[_rowIndex];
            string columnName = GetName(ordinal);
            if (row.TryGetValue(columnName, out object value))
            {
                return value;
            }
        }

        // Then check if we have a value set for this column name
        string colName = GetName(ordinal);
        if (_valuesByColumnName.TryGetValue(colName, out object val))
        {
            return val;
        }

        return ordinal switch
        {
            0 => 1,  // Default to 1 for first column
            _ => DBNull.Value
        };
    }

    public override int GetValues(object[] values)
    {
        int count = Math.Min(values.Length, FieldCount);
        for (int i = 0; i < count; i++)
        {
            values[i] = GetValue(i);
        }
        return count;
    }

    public override bool IsDBNull(int ordinal) => GetValue(ordinal) == DBNull.Value;

    public override System.Collections.IEnumerator GetEnumerator()
    {
        if (_rows.Count > 0)
        {
            return ((System.Collections.IEnumerable)_rows).GetEnumerator();
        }

        return new EmptyEnumerator();
    }
}
