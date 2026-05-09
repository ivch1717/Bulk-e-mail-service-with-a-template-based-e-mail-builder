namespace ConstructorUseCases.GetAllFiles;

public interface IGetAllFilesRequestHandler
{
    GetAllFilesResponse Handle(GetAllFilesRequest request);
}
