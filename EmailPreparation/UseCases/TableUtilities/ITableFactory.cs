using Microsoft.AspNetCore.Http;

namespace UseCases.TableUtilities;

/// <summary>
/// Интерфейс фабрики создания таблиц.
/// </summary>
public interface ITableFactory
{
    public ITable Create(IFormFile file);
    
    public ITable Create(IFormFile file, int from);
}