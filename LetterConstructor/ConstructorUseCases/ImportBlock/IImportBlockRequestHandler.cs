namespace ConstructorUseCases.ImportBlock;

public interface IImportBlockRequestHandler
{
    public ImportBlockResponse Handle(ImportBlockRequest request);
}