namespace UseCases;

public interface ITemplateParser
{
    public string Paste(string text, List<List<string>> data, int i);
}