namespace UseCases.Preparation.GetPreview;

public interface IGetPreviewRequestHandler
{
    public GetPreviewResponse Handle(GetPreviewRequest request);
}