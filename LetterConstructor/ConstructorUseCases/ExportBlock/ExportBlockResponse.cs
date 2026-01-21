namespace ConstructorUseCases.ExportBlock;

public record ExportBlockResponse(
    Stream Content,
    string FileName,
    string ContentType
    );