using Microsoft.AspNetCore.Http;
using Moq;
using UseCases;
using UseCases.Preparation.ExtractTableHeaders;

namespace Tests;

public class ExtractTableHeadersTests
{
    private readonly Mock<ITableFactory> _tableFactory = new();
    private readonly ExtractTableHeadersRequestHandler _handler;

    public ExtractTableHeadersTests()
    {
        _handler = new ExtractTableHeadersRequestHandler(_tableFactory.Object);
    }

    private IFormFile CreateMockFile() => new Mock<IFormFile>().Object;

    [Fact]
    public void Handle_WithHeaders_ReturnsHeaders()
    {
        var table = new Mock<ITable>();
        table.Setup(t => t.totalRows).Returns(1);
        table.Setup(t => t.GetRow(0, true)).Returns(["name", "email", "age"]);
        _tableFactory.Setup(f => f.Create(It.IsAny<IFormFile>())).Returns(table.Object);

        var result = _handler.Handle(new ExtractTableHeadersRequest(CreateMockFile()));

        Assert.Equal(["name", "email", "age"], result.headers);
    }

    [Fact]
    public void Handle_WithEmptyTable_ReturnsEmptyList()
    {
        var table = new Mock<ITable>();
        table.Setup(t => t.totalRows).Returns(0);
        _tableFactory.Setup(f => f.Create(It.IsAny<IFormFile>())).Returns(table.Object);

        var result = _handler.Handle(new ExtractTableHeadersRequest(CreateMockFile()));

        Assert.Empty(result.headers);
    }

    [Fact]
    public void Handle_WithEmptyFirstRows_ReturnsFirstNonEmptyRow()
    {
        var table = new Mock<ITable>();
        table.Setup(t => t.totalRows).Returns(3);
        table.Setup(t => t.GetRow(0, true)).Returns([]);
        table.Setup(t => t.GetRow(1, true)).Returns([]);
        table.Setup(t => t.GetRow(2, true)).Returns(["name", "email"]);
        _tableFactory.Setup(f => f.Create(It.IsAny<IFormFile>())).Returns(table.Object);

        var result = _handler.Handle(new ExtractTableHeadersRequest(CreateMockFile()));

        Assert.Equal(["name", "email"], result.headers);
    }
}