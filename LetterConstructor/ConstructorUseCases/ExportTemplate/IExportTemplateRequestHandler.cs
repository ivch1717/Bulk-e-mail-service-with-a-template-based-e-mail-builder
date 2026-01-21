namespace ConstructorUseCases.ExportTemplate;

public interface IExportTemplateRequestHandler
{
    public ExportTemplateResponse Handle(ExportTemplateRequest request);
}