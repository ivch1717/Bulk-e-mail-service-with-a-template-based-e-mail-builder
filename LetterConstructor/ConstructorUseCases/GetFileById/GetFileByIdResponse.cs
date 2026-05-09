namespace ConstructorUseCases.GetFileById;

public sealed record GetFileByIdResponse(Guid Id, string Name, string Content, DateTime CreatedAt);
