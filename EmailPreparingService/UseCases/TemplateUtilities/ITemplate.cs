namespace UseCases.TemplateUtilities;

public interface ITemplate
{
    public string CreateEmail(RowData rowData);
}