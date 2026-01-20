using Microsoft.AspNetCore.Http;

namespace UseCases;

public class AddressParser : IAddressParser
{
    public List<string> Parse(IFormFile data)
    {
        return ["email@mail.com"];
    }
}