namespace Genetec.Services.Entities;

public class Entity
{
    public Guid Guid { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public byte Type { get; set; }
    public byte SubType { get; set; }
    public Guid? CustomType { get; set; }
    public int? Version { get; set; }
    public DateTime CreationTime { get; set; }
    public long Flags { get; set; }
    public int? LogicalID { get; set; }
    public string? Info { get; set; }
    public string? CustomIcon { get; set; }
    public bool HiddenFromUI { get; set; }
    public bool Federated { get; set; }

    public ICollection<Cardholder> Cardholders { get; set; } = [];
}