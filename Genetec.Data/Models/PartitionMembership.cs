using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Genetec.Data.Models;

[PrimaryKey("GuidGroup", "GuidMember")]
[Table("PartitionMembership")]
[Index("GuidMember", Name = "IX_PartitionMembership_GuidMember")]
public record PartitionMembership
{
    [Key] public Guid GuidGroup { get; set; }

    [Key] public Guid GuidMember { get; set; }
}