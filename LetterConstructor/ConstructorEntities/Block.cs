namespace ConstructorEntities;

public class Block
{
    public Guid Id { get; }
    public int Order { get; private set; }
    public string Html { get; private set; }
    
    public Block(Guid id, int order, string html)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Block Id invalid");
        }

        if (order < 0)
        {
            throw new ArgumentException("Block Order invalid( < 0 )");
        }

        if (string.IsNullOrWhiteSpace(html))
        {
            throw new ArgumentException("Html empty");
        }
        Id = id;
        Order = order;
        Html = html;
    }
    
    public void UpdateHtml(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            throw new ArgumentException("Html empty");   
        }
        Html = html;
    }

    public void SetOrder(int order)
    {
        if (order < 0)
        {
            throw new ArgumentException("Block Order invalid( < 0 )");
        }
        Order = order;
    }
}