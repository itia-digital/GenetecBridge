using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace UP.Data.Models;

[Keyless]
public partial class PsUpIdGralTVw
{
    [Column("EMPLID")]
    [StringLength(11)]
    [Unicode(false)]
    public string Emplid { get; set; } = null!;

    [Column("FIRST_NAME")]
    [StringLength(30)]
    [Unicode(false)]
    public string FirstName { get; set; } = null!;

    [Column("LAST_NAME")]
    [StringLength(30)]
    [Unicode(false)]
    public string LastName { get; set; } = null!;

    [Column("SECOND_LAST_NAME")]
    [StringLength(30)]
    [Unicode(false)]
    public string SecondLastName { get; set; } = null!;

    [Column("SEX")]
    [StringLength(1)]
    [Unicode(false)]
    public string Sex { get; set; } = null!;

    [Column("INSTITUTION")]
    [StringLength(10)]
    [Unicode(false)]
    public string Institution { get; set; } = null!;

    [Column("EMAILID")]
    [StringLength(70)]
    [Unicode(false)]
    public string? Emailid { get; set; }

    [Column("ACAD_PROG")]
    [StringLength(5)]
    [Unicode(false)]
    public string AcadProg { get; set; } = null!;

    [Column("DESCR")]
    [StringLength(30)]
    [Unicode(false)]
    public string Descr { get; set; } = null!;

    [Column("GP_PAYGROUP")]
    [StringLength(10)]
    [Unicode(false)]
    public string GpPaygroup { get; set; } = null!;

    [Column("STATUS_FIELD")]
    [StringLength(4)]
    [Unicode(false)]
    public string StatusField { get; set; } = null!;

    [Column("PROG_REASON")]
    [StringLength(4)]
    [Unicode(false)]
    public string ProgReason { get; set; } = null!;

    [Column("LASTUPDDTTM", TypeName = "datetime")]
    public DateTime? Lastupddttm { get; set; }

    [Column("ASGMT_TYPE")]
    [StringLength(4)]
    [Unicode(false)]
    public string AsgmtType { get; set; } = null!;
}
