using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;

namespace UseCases.TemplateUtilities;

public class HtmlTemplate : ITemplate
{
    private string _text = "";
    
    public HtmlTemplate(IFormFile file)
    {
        using var reader = new StreamReader(file.OpenReadStream());
        _text = reader.ReadToEnd();
    }
    
    public string CreateEmail(RowData rowData, Dictionary<string, string> mapping)
    {
        return Regex.Replace(_text, @"\[\[(.*?)\]\]", match =>
        {
            string key = match.ToString().Substring(2, match.ToString().Length - 4);
            return rowData.data[mapping[key]];
        });
    }
}