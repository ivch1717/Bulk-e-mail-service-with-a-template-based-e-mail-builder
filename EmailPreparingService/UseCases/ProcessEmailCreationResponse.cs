namespace UseCases;

public record ProcessEmailCreationResponse(
    IReadOnlyList<string> emails,
    IReadOnlyList<string> text
);