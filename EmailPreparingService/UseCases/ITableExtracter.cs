using Microsoft.AspNetCore.Http;

namespace UseCases;

public interface ITableExtracter
{
    public List<string> Extract(IFormFile data, TableInfo tableInfo);
}