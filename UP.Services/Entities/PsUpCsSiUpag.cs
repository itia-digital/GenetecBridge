using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace UP.Services.Entities;

[Keyless]
public partial class PsUpCsSiUpag
{
    [Column("EMPLID")]
    [StringLength(11)]
    [Unicode(false)]
    public string Emplid { get; set; } = null!;

    [Column("INSTITUTION")]
    [StringLength(5)]
    [Unicode(false)]
    public string Institution { get; set; } = null!;

    [Column("SRVC_IND_CD")]
    [StringLength(3)]
    [Unicode(false)]
    public string SrvcIndCd { get; set; } = null!;

    [Column("DESCR")]
    [StringLength(30)]
    [Unicode(false)]
    public string Descr { get; set; } = null!;

    [Column("SRVC_IND_REASON")]
    [StringLength(5)]
    [Unicode(false)]
    public string SrvcIndReason { get; set; } = null!;

    [Column("DESCR1")]
    [StringLength(30)]
    [Unicode(false)]
    public string Descr1 { get; set; } = null!;
}
