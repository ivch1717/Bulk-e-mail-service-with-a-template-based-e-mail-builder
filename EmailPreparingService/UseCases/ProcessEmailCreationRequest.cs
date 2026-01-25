using Microsoft.AspNetCore.Http;

namespace UseCases;

public record ProcessEmailCreationRequest(
    IFormFile template,
    IFormFile data,
    string tableInfosJson
    );