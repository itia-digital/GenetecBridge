using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace UP.Data.Entities;

[Keyless]
public partial class PsUpIdGralVw
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

    [Column("UP_COUNT_PROG")]
    public int? UpCountProg { get; set; }

    [Column("UP_COUNT_EGR")]
    public int? UpCountEgr { get; set; }

    [Column("UP_COUNT_RH")]
    public int? UpCountRh { get; set; }

    [Column("UP_COUNT_SI")]
    public int? UpCountSi { get; set; }

    [Column("INSTITUTION")]
    [StringLength(5)]
    [Unicode(false)]
    public string Institution { get; set; } = null!;
}
