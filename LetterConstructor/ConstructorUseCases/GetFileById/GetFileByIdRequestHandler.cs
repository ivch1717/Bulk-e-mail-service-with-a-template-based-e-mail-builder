namespace ConstructorUseCases.GetFileById;

public class GetFileByIdRequestHandler : IGetFileByIdRequestHandler
{
    private readonly IGetFileByIdRepository _repository;

    public GetFileByIdRequestHandler(IGetFileByIdRepository repository)
    {
        _repository = repository;
    }

    public GetFileByIdResponse Handle(GetFileByIdRequest request)
    {
        var file = _repository.GetFileById(request.FileId);
        if (file is null)
        {
            throw new KeyNotFoundException($"Файл с Id {request.FileId} не найден");
        }

        return GetFileByIdMapper.ToResponse(file);
    }
}
