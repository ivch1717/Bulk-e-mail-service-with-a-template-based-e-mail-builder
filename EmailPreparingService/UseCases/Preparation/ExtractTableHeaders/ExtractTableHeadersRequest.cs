using Microsoft.AspNetCore.Http;

namespace UseCases.Preparation.ExtractTableHeaders;

/// <summary>
/// Запрос для получения заголовков таблицы.
/// </summary>
/// <param name="table">Таблица с данными для подстановки.</param>
public record ExtractTableHeadersRequest(IFormFile table);