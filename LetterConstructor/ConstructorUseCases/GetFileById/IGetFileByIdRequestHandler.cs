namespace ConstructorUseCases.GetFileById;

public interface IGetFileByIdRequestHandler
{
    GetFileByIdResponse Handle(GetFileByIdRequest request);
}
