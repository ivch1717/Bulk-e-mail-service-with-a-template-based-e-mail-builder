using Microsoft.AspNetCore.Http;
using Moq;
using UseCases;
using UseCases.Preparation.GetPreview;
using UseCases.TableUtilities;
using UseCases.TemplateUtilities;

namespace Tests;

public class GetPreviewTests
{
    private readonly Mock<ITableFactory> _tableFactory = new();
    private readonly Mock<ITemplateFactory> _templateFactory = new();
    private readonly GetPreviewRequestHandler _handler;

    public GetPreviewTests()
    {
        _handler = new GetPreviewRequestHandler(_tableFactory.Object, _templateFactory.Object);
    }

    private IFormFile CreateMockFile() => new Mock<IFormFile>().Object;

    [Fact]
    public void Handle_WithValidData_ReturnsEmailPreviews()
    {
        var table = new Mock<ITable>();
        var template = new Mock<ITemplate>();

        table.Setup(t => t.GetTotal(It.IsAny<HashSet<string>>())).Returns(2);
        table.Setup(t => t.GetData(It.IsAny<HashSet<string>>(), It.IsAny<int>())).Returns([
            new RowData(new Dictionary<string, string> { ["email"] = "test@example.com", ["name"] = "John" }),
            new RowData(new Dictionary<string, string> { ["email"] = "test2@example.com", ["name"] = "Jane" })
        ]);
        table.Setup(t => t.CurrentRow).Returns(2);

        template.Setup(t => t.CreateEmail(It.IsAny<RowData>(), It.IsAny<Dictionary<string, string>>()))
            .Returns("<p>Hello</p>");

        _tableFactory.Setup(f => f.Create(It.IsAny<IFormFile>())).Returns(table.Object);
        _templateFactory.Setup(f => f.Create(It.IsAny<IFormFile>(), false)).Returns(template.Object);

        var request = new GetPreviewRequest(
            CreateMockFile(), CreateMockFile(),
            from: null, count: 10,
            mappingJson: """{"email":"email","name":"name"}"""
        );

        var result = _handler.Handle(request);

        Assert.Equal(2, result.emailPreviews.Count);
        Assert.Equal(2, result.nextRow);
    }

    [Fact]
    public void Handle_WithInvalidEmail_SkipsRow()
    {
        var table = new Mock<ITable>();
        var template = new Mock<ITemplate>();

        table.Setup(t => t.GetTotal(It.IsAny<HashSet<string>>())).Returns(2);
        table.Setup(t => t.GetData(It.IsAny<HashSet<string>>(), It.IsAny<int>())).Returns([
            new RowData(new Dictionary<string, string> { ["email"] = "not-an-email", ["name"] = "John" }),
            new RowData(new Dictionary<string, string> { ["email"] = "valid@example.com", ["name"] = "Jane" })
        ]);
        table.Setup(t => t.CurrentRow).Returns(2);

        template.Setup(t => t.CreateEmail(It.IsAny<RowData>(), It.IsAny<Dictionary<string, string>>()))
            .Returns("<p>Hello</p>");

        _tableFactory.Setup(f => f.Create(It.IsAny<IFormFile>())).Returns(table.Object);
        _templateFactory.Setup(f => f.Create(It.IsAny<IFormFile>(), false)).Returns(template.Object);

        var request = new GetPreviewRequest(
            CreateMockFile(), CreateMockFile(),
            from: null, count: 10,
            mappingJson: """{"email":"email","name":"name"}"""
        );

        var result = _handler.Handle(request);

        Assert.Single(result.emailPreviews);
        Assert.Equal(1, result.total);
    }

    [Fact]
    public void Handle_WithFromParameter_DoesNotReturnTotal()
    {
        var table = new Mock<ITable>();
        var template = new Mock<ITemplate>();

        table.Setup(t => t.GetData(It.IsAny<HashSet<string>>(), It.IsAny<int>())).Returns([]);
        table.Setup(t => t.CurrentRow).Returns(5);

        _tableFactory.Setup(f => f.Create(It.IsAny<IFormFile>(), 5)).Returns(table.Object);
        _templateFactory.Setup(f => f.Create(It.IsAny<IFormFile>(), false)).Returns(template.Object);

        var request = new GetPreviewRequest(
            CreateMockFile(), CreateMockFile(),
            from: 5, count: 10,
            mappingJson: """{"email":"email"}"""
        );

        var result = _handler.Handle(request);

        Assert.Null(result.total);
    }
}