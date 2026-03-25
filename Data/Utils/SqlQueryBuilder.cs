namespace Data.Utils;

public class SqlQueryBuilder
{
    private readonly StringBuilder _where = new();
    private bool _hasWhere;

    public DynamicParameters Parameters { get; } = new();

    public void Where(string condition, string? paramName = null, object? value = null)
    {
        _where.Append(_hasWhere ? " AND " : " WHERE ");
        _where.Append(condition);
        _hasWhere = true;

        if (paramName != null)
            Parameters.Add(paramName, value);
    }

    public string Build(string baseQuery)
        => baseQuery + _where;
}
