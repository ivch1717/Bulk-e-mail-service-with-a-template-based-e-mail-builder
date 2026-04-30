using Microsoft.AspNetCore.Http;

namespace UseCases;

/// <summary>
/// Обычная фабрика таблиц.
/// </summary>
public class TableFactory : ITableFactory
{
    /// <summary>
    /// Создает нужную таблицу в зависимости от содержимого файла.
    /// </summary>
    /// <param name="file">Файл таблицы.</param>
    /// <returns>Объект ITable.</returns>
    public ITable Create(IFormFile file)
    {
        // TODO: сделать поддержку других форматов.
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        return extension switch
        {
            ".xlsx" => new XlsxTable(file),
            _ => throw new ArgumentException($"Неподдерживаемый формат файла: {extension}.")
        };
    }
    
    public ITable Create(IFormFile file, int from)
    {
        // TODO: сделать поддержку других форматов.
        return new XlsxTable(file, from);
    }
}