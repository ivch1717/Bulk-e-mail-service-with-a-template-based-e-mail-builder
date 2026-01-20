using Microsoft.AspNetCore.Http;

namespace UseCases;

public class UploadDataRequestHandler : IUploadDataRequestHandler
{
    IAddressParser _addressParser;

    public UploadDataRequestHandler(IAddressParser addressParser)
    {
        _addressParser = addressParser;
    }
    
    public UploadDataResponse Handle(UploadDataRequest request)
    {
        List<string> adresses = _addressParser.Parse(request.data);
        List<IFormFile> emails = [];
        for (int i = 0; i < adresses.Count; i++)
        {
            emails.Add(request.template);
        }
        return new UploadDataResponse(adresses, emails);
    }
}