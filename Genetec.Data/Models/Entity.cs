using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Genetec.Data.Models;

[Table("Entity")]
[Index("UpId", Name = "Entity_UpId_index")]
[Index("Type", "SubType", "Guid", Name = "IX_EntityType")]
public record Entity
{
    [Key] public Guid Guid { get; set; }

    [StringLength(100)] public string Name { get; set; } = null!;

    [StringLength(100)] public string Description { get; set; } = null!;

    public byte Type { get; set; }

    public byte SubType { get; set; }

    public Guid? CustomType { get; set; }

    public int? Version { get; set; }

    [Column(TypeName = "datetime")] public DateTime CreationTime { get; set; }

    public long Flags { get; set; }

    [Column("LogicalID")] public int? LogicalId { get; set; }

    public string? Info { get; set; }

    public string? CustomIcon { get; set; }

    [Column("HiddenFromUI")] public bool? HiddenFromUi { get; set; }

    public bool? Federated { get; set; }

    [StringLength(10)] public string? UpId { get; set; }

    [Column(TypeName = "datetime")] public DateTime? SyncedAt { get; set; }

    [InverseProperty("Escort2Navigation")]
    public virtual ICollection<Cardholder> CardholderEscort2Navigations { get; set; } =
        new List<Cardholder>();

    [InverseProperty("EscortNavigation")]
    public virtual ICollection<Cardholder> CardholderEscortNavigations { get; set; } =
        new List<Cardholder>();

    [InverseProperty("Gu")] public virtual Cardholder? CardholderGu { get; set; }

    [InverseProperty("GuidMemberNavigation")]
    public virtual ICollection<CardholderMembership> CardholderMemberships { get; set; } =
        new List<CardholderMembership>();
}