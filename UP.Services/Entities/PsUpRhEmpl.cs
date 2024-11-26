using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace UP.Services.Entities;

[Keyless]
public partial class PsUpRhEmpl
{
    [Column("EMPLID")]
    [StringLength(11)]
    [Unicode(false)]
    public string Emplid { get; set; } = null!;

    [Column("EMPL_RCD")]
    public short EmplRcd { get; set; }

    [Column("EFFDT", TypeName = "datetime")]
    public DateTime Effdt { get; set; }

    [Column("EFFSEQ")]
    public short Effseq { get; set; }

    [Column("EMPL_STATUS")]
    [StringLength(1)]
    [Unicode(false)]
    public string EmplStatus { get; set; } = null!;

    [Column("LOCATION")]
    [StringLength(10)]
    [Unicode(false)]
    public string Location { get; set; } = null!;

    [Column("GP_PAYGROUP")]
    [StringLength(10)]
    [Unicode(false)]
    public string GpPaygroup { get; set; } = null!;

    [Column("FTE", TypeName = "numeric(7, 6)")]
    public decimal Fte { get; set; }

    [Column("EMPL_CLASS")]
    [StringLength(3)]
    [Unicode(false)]
    public string EmplClass { get; set; } = null!;
}
