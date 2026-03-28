namespace UseCases.ExtractTableHeaders;

/// <summary>
/// Интерфейс для обработчика.
/// </summary>
public interface IExtractTableHeadersRequestHandler 
{
    public ExtractTableHeadersResponse Handle(ExtractTableHeadersRequest request);
}