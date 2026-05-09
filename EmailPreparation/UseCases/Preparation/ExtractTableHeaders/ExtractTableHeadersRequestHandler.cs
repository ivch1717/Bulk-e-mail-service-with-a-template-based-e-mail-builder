using UseCases.TableUtilities;

namespace UseCases.Preparation.ExtractTableHeaders;

/// <summary>
/// Обработчик для получения заголовков таблицы.
/// </summary>
public class ExtractTableHeadersRequestHandler(ITableFactory tableFactory) : IExtractTableHeadersRequestHandler
{
    /// <summary>
    /// Фабрика таблиц.
    /// </summary>
    private readonly ITableFactory _tableFactory = tableFactory;

    /// <summary>
    /// Ищет заголовки столбцов, просматривая до первой не пустой строки в первом листе таблицы.
    /// </summary>
    /// <param name="request">Таблица.</param>
    /// <returns>Заголовки.</returns>
    public ExtractTableHeadersResponse Handle(ExtractTableHeadersRequest request)
    {
        var table = _tableFactory.Create(request.table);
        for (var i = 0; i < table.totalRows; ++i)
        {
            var result = table.GetRow(i, skipEmpty: true);
            
            var duplicates = result
                .GroupBy(h => h)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicates.Count > 0)
            { 
                throw new ArgumentException(
                    $"В заголовке таблицы есть дубликаты столбцов: {string.Join(", ", duplicates)}. Их необходимо убрать.");
            }
            
            if (result.Count != 0)
            {
                return new ExtractTableHeadersResponse(result);
            }
        }

        throw new ArgumentException("В таблице нет непустых строк.");
    }
}