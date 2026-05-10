using Entities;

namespace ConstructureInfrastructure.Data.Dtos;

public record HtmlFileDto(Guid Id, string Name, string Content, DateTime CreatedAt, HtmlFileType Type);
