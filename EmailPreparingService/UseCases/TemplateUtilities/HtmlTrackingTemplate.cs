using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;

namespace UseCases.TemplateUtilities;

public class HtmlTrackingTemplate : ITemplate
{
    private string _text = "";

    private const string trackingTemplate =
        $"<img src=\"http://77.105.170.38/api/track/open?campaignId=[[campaignId]]&email=[[email]]\" width=\"1\" height=\"1\" style=\"display:none\" alt=\"\" />";
    public HtmlTrackingTemplate(IFormFile file)
    {
        using var reader = new StreamReader(file.OpenReadStream());
        _text = reader.ReadToEnd();
    }
    
    public string CreateEmail(RowData rowData, Dictionary<string, string> mapping)
    {
        string text = Regex.Replace(_text, @"\[\[(.*?)\]\]", match =>
        {
            string key = match.ToString().Substring(2, match.ToString().Length - 4);
            return rowData.data[mapping[key]];
        });
        return text + trackingTemplate;
    }
}