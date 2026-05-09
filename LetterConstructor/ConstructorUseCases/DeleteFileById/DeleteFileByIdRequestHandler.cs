namespace ConstructorUseCases.DeleteFileById;

public class DeleteFileByIdRequestHandler : IDeleteFileByIdRequestHandler
{
    private readonly IDeleteFileByIdRepository _repository;

    public DeleteFileByIdRequestHandler(IDeleteFileByIdRepository repository)
    {
        _repository = repository;
    }

    public DeleteFileByIdResponse Handle(DeleteFileByIdRequest request)
    {
        if (!_repository.ExistsById(request.FileId))
        {
            throw new KeyNotFoundException($"Файл с Id {request.FileId} не найден");
        }

        _repository.DeleteFileById(request.FileId);
        return new DeleteFileByIdResponse(request.FileId);
    }
}
