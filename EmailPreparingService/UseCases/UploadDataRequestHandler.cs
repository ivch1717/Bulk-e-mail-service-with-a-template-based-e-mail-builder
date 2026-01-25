// using Microsoft.AspNetCore.Http;
//
// namespace UseCases;
//
// public class UploadDataRequestHandler : IUploadDataRequestHandler
// {
//     IAddressParser _addressParser;
//     ITemplateParser _templateParser;
//     IDataParser _dataParser;
//
//     public UploadDataRequestHandler(IAddressParser addressParser, ITemplateParser templateParser, IDataParser dataParser)
//     {
//         _addressParser = addressParser;
//         _templateParser = templateParser;
//         _dataParser = dataParser;
//     }
//     
//     public UploadDataResponse Handle(UploadDataRequest request)
//     {
//         List<string> adresses = _addressParser.Parse(request.data);
//         List<string> emails = [];
//         for (int i = 0; i < adresses.Count; i++)
//         {
//             emails.Add(_templateParser.Paste(ReadText(request.template), _dataParser.Parse(request.data, i)));
//         }
//         return new UploadDataResponse(adresses, emails);
//     }
//     
//     public string ReadText(IFormFile file)
//     {
//         using var reader = new StreamReader(file.OpenReadStream());
//         return reader.ReadToEnd();
//     }
// }