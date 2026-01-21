namespace ConstructorUseCases.ExportTemplate;

public record ExportTemplateResponse(
    Stream Content,
    string FileName,
    string ContentType
    );