using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Genetec.Data.Models;

[Keyless]
[Table("AlusaControl")]
[Index("EntityGuid", Name = "AlusaControlIds_EntityGuid_uindex", IsUnique = true)]
public partial class AlusaControl
{
    public Guid EntityGuid { get; set; }

    [StringLength(10)]
    [Unicode(false)]
    public string? UpId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? UpdatedAt { get; set; }
}
