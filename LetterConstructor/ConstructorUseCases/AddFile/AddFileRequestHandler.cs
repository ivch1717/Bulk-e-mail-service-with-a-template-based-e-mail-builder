namespace ConstructorUseCases.AddFile;

public class AddFileRequestHandler : IAddFileRequestHandler
{
    private readonly IAddFileRepository _repository;

    public AddFileRequestHandler(IAddFileRepository repository)
    {
        _repository = repository;
    }

    public AddFileResponse Handle(AddFileRequest request)
    {
        var file = AddFileMapper.ToEntity(request, Guid.NewGuid());
        _repository.AddFile(file);
        return new AddFileResponse(file.Id);
    }
}
