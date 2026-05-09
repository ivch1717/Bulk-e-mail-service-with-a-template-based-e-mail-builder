namespace ConstructorUseCases.GetAllFiles;

public class GetAllFilesRequestHandler : IGetAllFilesRequestHandler
{
    private readonly IGetAllFilesRepository _repository;

    public GetAllFilesRequestHandler(IGetAllFilesRepository repository)
    {
        _repository = repository;
    }

    public GetAllFilesResponse Handle(GetAllFilesRequest request)
    {
        var files = _repository.GetAllFiles();
        return new GetAllFilesResponse(files);
    }
}
