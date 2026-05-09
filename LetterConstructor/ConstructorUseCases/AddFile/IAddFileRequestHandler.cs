namespace ConstructorUseCases.AddFile;

public interface IAddFileRequestHandler
{
    AddFileResponse Handle(AddFileRequest request);
}
