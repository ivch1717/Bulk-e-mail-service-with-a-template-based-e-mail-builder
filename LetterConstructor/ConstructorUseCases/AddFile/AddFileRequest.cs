namespace ConstructorUseCases.AddFile;

public sealed record AddFileRequest(string Name, string Content, DateTime CreatedAt);
