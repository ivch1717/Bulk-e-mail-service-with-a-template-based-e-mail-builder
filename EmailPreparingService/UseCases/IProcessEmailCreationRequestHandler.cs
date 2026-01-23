namespace UseCases;

public interface IProcessEmailCreationRequestHandler
{
    ProcessEmailCreationResponse Handle(ProcessEmailCreationRequest request);
}