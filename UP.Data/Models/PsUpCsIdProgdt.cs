using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace UP.Data.Models;

[Keyless]
public partial class PsUpCsIdProgdt
{
    [Column("EMPLID")]
    [StringLength(11)]
    [Unicode(false)]
    public string Emplid { get; set; } = null!;

    [Column("INSTITUTION")]
    [StringLength(5)]
    [Unicode(false)]
    public string Institution { get; set; } = null!;

    [Column("ACAD_PROG")]
    [StringLength(5)]
    [Unicode(false)]
    public string AcadProg { get; set; } = null!;

    [Column("DESCR")]
    [StringLength(30)]
    [Unicode(false)]
    public string Descr { get; set; } = null!;

    [Column("ACAD_GROUP")]
    [StringLength(5)]
    [Unicode(false)]
    public string AcadGroup { get; set; } = null!;

    [Column("PROG_STATUS")]
    [StringLength(4)]
    [Unicode(false)]
    public string ProgStatus { get; set; } = null!;

    [Column("EFFDT", TypeName = "datetime")]
    public DateTime Effdt { get; set; }

    [Column("OPRID")]
    [StringLength(30)]
    [Unicode(false)]
    public string? Oprid { get; set; }

    [Column("ACTION_DT", TypeName = "datetime")]
    public DateTime ActionDt { get; set; }
}
