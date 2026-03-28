using Microsoft.AspNetCore.Http;

namespace UseCases;

/// <summary>
/// Интерфейс фабрики создания таблиц.
/// </summary>
public interface ITableFactory
{
    public ITable Create(IFormFile file);
}