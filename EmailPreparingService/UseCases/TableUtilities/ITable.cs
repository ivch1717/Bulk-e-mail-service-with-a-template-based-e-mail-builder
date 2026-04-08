namespace UseCases;

/// <summary>
/// Обертка для работы с разными форматами таблиц.
/// </summary>
public interface ITable
{
    public int totalRows { get; }
    public int CurrentRow { get; }
    public List<string> GetRow(int rowNumber, bool skipEmpty = true);
    public List<RowData> GetData(HashSet<string> columns, int count);
    public int GetTotal(HashSet<string> columns);
}