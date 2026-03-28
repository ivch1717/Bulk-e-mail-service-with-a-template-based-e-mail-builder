using Microsoft.AspNetCore.Http;

namespace UseCases.ExtractTableHeaders;

/// <summary>
/// Запрос для получения заголовков столбцов.
/// </summary>
/// <param name="table">Таблица с данными для подстановки.</param>
public record ExtractTableHeadersRequest(
    IFormFile table
    );