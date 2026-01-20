using Microsoft.AspNetCore.Http;

namespace UseCases;

public interface IAddressParser
{
    List<string> Parse(IFormFile data);
}