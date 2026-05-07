namespace UseCases.Preparation.GetPreview;

public record GetPreviewResponse(
    List<EmailPreview> emailPreviews,
    int nextRow,
    int? total
);