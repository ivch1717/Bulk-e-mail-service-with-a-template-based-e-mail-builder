namespace UseCases;

public interface IUploadDataRequestHandler
{
    UploadDataResponse Handle(UploadDataRequest request);
}