using ConstructureInfrastructure.Data.Dtos;
using Entities;

namespace ConstructureInfrastructure.Data.Mappers;

public static class DataMapper
{
    public static HtmlFileDto ToDto(this HtmlFile file)
    {
        return new HtmlFileDto(
            file.Id,
            file.Name,
            file.Content,
            file.CreatedAt,
            file.Type);
    }

    public static HtmlFile ToEntity(this HtmlFileDto dto)
    {
        return new HtmlFile(
            dto.Id,
            dto.Name,
            dto.Content,
            dto.CreatedAt,
            dto.Type);
    }
}
