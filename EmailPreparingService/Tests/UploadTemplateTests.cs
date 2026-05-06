using Microsoft.AspNetCore.Http;
using Moq;
using UseCases.TemplateUtilities;
using UseCases.UploadTemplate;

namespace Tests;

public class UploadTemplateTests
{
    private readonly UploadTemplateRequestHandler _handler;

    public UploadTemplateTests()
    {
        var templateFactory = new Mock<ITemplateFactory>();
        _handler = new UploadTemplateRequestHandler(templateFactory.Object);
    }

    private static IFormFile CreateFormFile(string content)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(content);
        var stream = new MemoryStream(bytes);
        var file = new Mock<IFormFile>();
        file.Setup(f => f.OpenReadStream()).Returns(stream);
        return file.Object;
    }

    [Fact]
    public void Handle_WithVariables_ReturnsVariablesList()
    {
        var file = CreateFormFile("<p>Hello [[name]], your email is [[email]]</p>");
        var request = new UploadTemplateRequest(file);

        var result = _handler.Handle(request);

        Assert.Contains("name", result);
        Assert.Contains("email", result);
    }

    [Fact]
    public void Handle_WithoutVariables_ReturnsOnlyEmail()
    {
        var file = CreateFormFile("<p>Hello world</p>");
        var request = new UploadTemplateRequest(file);

        var result = _handler.Handle(request);

        Assert.Single(result);
        Assert.Contains("email", result);
    }

    [Fact]
    public void Handle_WithDuplicateVariables_ReturnsUniqueOnly()
    {
        var file = CreateFormFile("<p>[[name]] [[name]] [[name]]</p>");
        var request = new UploadTemplateRequest(file);

        var result = _handler.Handle(request);

        Assert.Equal(2, result.Count);
    }
}