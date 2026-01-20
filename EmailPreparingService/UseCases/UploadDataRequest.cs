using Microsoft.AspNetCore.Http;

namespace UseCases;

public sealed record UploadDataRequest (
    IFormFile template,
    IFormFile data
);