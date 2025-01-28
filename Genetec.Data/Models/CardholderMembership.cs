using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Genetec.Data.Models;

[PrimaryKey("GuidGroup", "GuidMember")]
[Table("CardholderMembership")]
[Index("GuidMember", Name = "IX_CardholderMembership_GuidMember")]
public record CardholderMembership
{
    [Key] public Guid GuidGroup { get; set; }

    [Key] public Guid GuidMember { get; set; }
    
    [StringLength(10)] public string? UpId { get; set; }

    [ForeignKey("GuidMember")]
    [InverseProperty("CardholderMemberships")]
    public virtual Entity GuidMemberNavigation { get; set; } = null!;
}