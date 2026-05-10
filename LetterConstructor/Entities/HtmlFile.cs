namespace Entities;

public class HtmlFile
{
    public Guid Id { get; }
    public string Name { get; }
    public string Content { get; }
    public DateTime CreatedAt { get; }
    public HtmlFileType Type { get; }

    public HtmlFile(Guid id, string name, string content, DateTime createdAt, HtmlFileType type)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Id не может быть пустым", nameof(id));
        }

        if (name == null)
        {
            throw new ArgumentNullException(nameof(name));
        }

        if (string.IsNullOrEmpty(content))
        {
            throw new ArgumentException("Content не может быть пустым", nameof(content));
        }

        if (createdAt == default)
        {
            throw new ArgumentException("CreatedAt не может быть пустым", nameof(createdAt));
        }

        Id = id;
        Name = name;
        Content = content;
        CreatedAt = createdAt;
        Type = type;
    }
}
