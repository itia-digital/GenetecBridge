using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AnthologySap.Models;

[Keyless]
public partial class VUsuariosUnificado
{
    [Column("EMPLID")]
    [StringLength(50)]
    public string? Emplid { get; set; }

    [Column("FIRST_NAME")]
    [StringLength(100)]
    public string? FirstName { get; set; }

    [Column("LAST_NAME")]
    [StringLength(100)]
    public string? LastName { get; set; }

    [Column("SECOND_LAST_NAME")]
    [StringLength(50)]
    [Unicode(false)]
    public string? SecondLastName { get; set; }

    [Column("SEX")]
    [StringLength(1)]
    [Unicode(false)]
    public string? Sex { get; set; }

    [Column("INSTITUTION")]
    [StringLength(50)]
    public string? Institution { get; set; }

    [Column("EMAILID")]
    [StringLength(100)]
    public string? Emailid { get; set; }

    [Column("ACAD_PROG")]
    [StringLength(50)]
    public string? AcadProg { get; set; }

    [Column("DESCR")]
    [StringLength(200)]
    public string? Descr { get; set; }

    [Column("GP_PAYGROUP")]
    [StringLength(25)]
    [Unicode(false)]
    public string? GpPaygroup { get; set; }

    [Column("STATUS_FIELD")]
    [StringLength(50)]
    public string? StatusField { get; set; }

    [Column("PROG_STATUS")]
    [StringLength(100)]
    public string? ProgStatus { get; set; }

    [Column("LASTUPDDTTM")]
    public DateTime? Lastupddttm { get; set; }

    [Column("ASGMT_TYPE")]
    [StringLength(25)]
    [Unicode(false)]
    public string? AsgmtType { get; set; }

    [Column("JOB_TITLE")]
    [StringLength(50)]
    [Unicode(false)]
    public string? JobTitle { get; set; }
}
