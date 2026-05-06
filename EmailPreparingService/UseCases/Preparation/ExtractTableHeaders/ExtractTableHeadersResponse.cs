namespace UseCases.Preparation.ExtractTableHeaders;

/// <summary>
/// Ответ для получения заголовков таблицы.
/// </summary>
/// <param name="headers">Заголовки.</param>
public record ExtractTableHeadersResponse(List<string> headers);