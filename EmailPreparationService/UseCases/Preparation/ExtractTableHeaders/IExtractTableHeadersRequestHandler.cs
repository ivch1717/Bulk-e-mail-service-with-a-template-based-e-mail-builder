namespace UseCases.Preparation.ExtractTableHeaders;

/// <summary>
/// Интерфейс для обработчика таблицы.
/// </summary>
public interface IExtractTableHeadersRequestHandler 
{
    public ExtractTableHeadersResponse Handle(ExtractTableHeadersRequest request);
}