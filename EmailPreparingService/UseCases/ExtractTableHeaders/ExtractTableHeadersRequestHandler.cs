namespace UseCases.ExtractTableHeaders;

/// <summary>
/// Обработчик для получения заголовков столбцов.
/// </summary>
public class ExtractTableHeadersRequestHandler : IExtractTableHeadersRequestHandler
{
    /// <summary>
    /// Фабрика таблиц для поддержки разных форматов таблиц.
    /// </summary>
    private ITableFactory _tableFactory;
    
    public ExtractTableHeadersRequestHandler(ITableFactory tableFactory)
    {
        _tableFactory = tableFactory;
    }
    
    /// <summary>
    /// Ищет заголовки столбцов, просматривая до первой не пустой строки в первом листе таблицы.
    /// </summary>
    /// <param name="request">Таблица с данными.</param>
    /// <returns>Список заголовков</returns>
    public ExtractTableHeadersResponse Handle(ExtractTableHeadersRequest request)
    {
        ITable table = _tableFactory.Create(request.table);
        for (int i = 0; i < table.totalRows; ++i)
        {
            List<string> result = table.GetRow(i, skipEmpty: true);
            if (result.Count != 0)
            {
                return new ExtractTableHeadersResponse(result);
            }
        }
        return new ExtractTableHeadersResponse(null);
    }
}