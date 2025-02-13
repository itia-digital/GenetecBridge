using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Genetec.Data.Models;

[Table("AlusaControl")]
[Index("Name", "EndedAt", Name = "AlusaControl_Name_EndedAt_index")]
public partial record AlusaControl
{
    [StringLength(50)]
    public string? Name { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? StartedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? EndedAt { get; set; }

    [Key]
    public int Id { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? SyncedDate { get; set; }
}
