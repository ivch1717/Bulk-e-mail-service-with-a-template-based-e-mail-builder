namespace UseCases.Preparation.Send;

public interface ISendRequestHandler
{
    public Task<SendResponse> Handle(SendRequest request);
}