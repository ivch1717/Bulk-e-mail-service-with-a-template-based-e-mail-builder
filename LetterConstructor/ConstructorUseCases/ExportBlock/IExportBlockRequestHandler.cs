namespace ConstructorUseCases.ExportBlock;

public interface IExportBlockRequestHandler
{
    public ExportBlockResponse Handle(ExportBlockRequest request);
}