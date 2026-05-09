namespace ConstructorUseCases.DeleteFileById;

public interface IDeleteFileByIdRequestHandler
{
    DeleteFileByIdResponse Handle(DeleteFileByIdRequest request);
}
