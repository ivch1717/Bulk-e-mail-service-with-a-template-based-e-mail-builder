using System.Text.RegularExpressions;

namespace UseCases;

public class TemplateParser : ITemplateParser
{
    public string Paste(string text, List<List<string>> data, int i)
    {
        int c = 0;
        return Regex.Replace(text, @"\[\[(.*?)\]\]", match =>
        {
            return data[c++][i];
        });
    }
}