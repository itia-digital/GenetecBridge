using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace UP.Services.Entities;

[Keyless]
public partial class PsUpRhIdDeptvw
{
    [Column("EMPLID")]
    [StringLength(11)]
    [Unicode(false)]
    public string Emplid { get; set; } = null!;

    [Column("DESCR")]
    [StringLength(30)]
    [Unicode(false)]
    public string Descr { get; set; } = null!;

    [Column("JOBCODE_DESCR")]
    [StringLength(30)]
    [Unicode(false)]
    public string JobcodeDescr { get; set; } = null!;

    [Column("GP_PAYGROUP")]
    [StringLength(10)]
    [Unicode(false)]
    public string GpPaygroup { get; set; } = null!;

    [Column("DESCR1")]
    [StringLength(30)]
    [Unicode(false)]
    public string Descr1 { get; set; } = null!;

    [Column("EFFDT", TypeName = "datetime")]
    public DateTime Effdt { get; set; }

    [Column("HR_STATUS")]
    [StringLength(1)]
    [Unicode(false)]
    public string HrStatus { get; set; } = null!;
}
