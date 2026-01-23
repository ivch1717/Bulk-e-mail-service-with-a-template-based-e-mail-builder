namespace UseCases;

public interface IUploadTemplateRequestHandler
{
    public List<string> Handle(UploadTemplateRequest request);
}