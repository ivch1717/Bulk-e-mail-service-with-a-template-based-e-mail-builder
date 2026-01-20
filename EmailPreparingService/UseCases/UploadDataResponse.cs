using Microsoft.AspNetCore.Http;

namespace UseCases;

public sealed record UploadDataResponse (
    IReadOnlyList<string> emails,
    IReadOnlyList<IFormFile> text
);