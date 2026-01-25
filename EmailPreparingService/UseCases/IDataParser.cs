using Microsoft.AspNetCore.Http;

namespace UseCases;

public interface IDataParser
{
    public List<string> Parse(IFormFile file, int column);
}