namespace ConstructorEntities;

public class EmailTemplate
{
    public Guid Id { get; }
    
    public IReadOnlyCollection<Block> Blocks => _blocks.AsReadOnly();
    private readonly List<Block> _blocks;

    public EmailTemplate(Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Block ID cannot be empty");
        }
        Id = id;
        _blocks = new List<Block>();
    }

    public void AddBlock(Block block)
    {
        if (block is null)
        {
            throw new ArgumentNullException(nameof(block));
        }

        if (_blocks.Any(x => x.Id == block.Id))
        {
            throw new ArgumentException("Duplicate block id");
        }
        
        _blocks.Add(block);
    }

    public void RemoveBlock(Guid blockId)
    {
        var block = _blocks.FirstOrDefault(x => x.Id == blockId);
        if (block is null)
        {
            throw new ArgumentException("Block not found");
        }
        _blocks.Remove(block);
        NormalizeOrder();
    }

    public void NormalizeOrder()
    {
        var ordered = _blocks.OrderBy(b => b.Order).ToList();
        for (int i = 0; i < ordered.Count; i++)
        {
            ordered[i].SetOrder(i);
        }
    }
}