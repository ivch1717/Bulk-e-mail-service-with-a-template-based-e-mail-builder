namespace UseCases;

/// <summary>
/// Обертка для работы с разными форматами таблиц.
/// </summary>
public interface ITable
{
    public int totalRows { get; }
    public List<string> GetRow(int rowNumber, bool skipEmpty = true);
}