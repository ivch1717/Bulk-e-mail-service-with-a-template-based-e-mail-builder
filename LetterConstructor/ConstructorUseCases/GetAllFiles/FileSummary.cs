using Entities;

namespace ConstructorUseCases.GetAllFiles;

public sealed record FileSummary(Guid Id, string Name, DateTime CreatedAt, HtmlFileType Type);
