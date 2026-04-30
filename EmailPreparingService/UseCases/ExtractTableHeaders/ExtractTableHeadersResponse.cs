namespace UseCases.ExtractTableHeaders;

/// <summary>
/// Ответ для получения заголовков таблицы.
/// </summary>
/// <param name="headers">Список заголовков.</param>
public record ExtractTableHeadersResponse(List<string> headers);