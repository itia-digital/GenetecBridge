using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace UP.Data.Models;

[Keyless]
public partial class PsUpPersonalMod
{
    [Column("EMPLID")]
    [StringLength(11)]
    [Unicode(false)]
    public string Emplid { get; set; } = null!;

    [Column("OPRID")]
    [StringLength(30)]
    [Unicode(false)]
    public string? Oprid { get; set; }

    [Column("LAST_NAME")]
    [StringLength(30)]
    [Unicode(false)]
    public string LastName { get; set; } = null!;

    [Column("SECOND_LAST_NAME")]
    [StringLength(30)]
    [Unicode(false)]
    public string SecondLastName { get; set; } = null!;

    [Column("FIRST_NAME")]
    [StringLength(30)]
    [Unicode(false)]
    public string FirstName { get; set; } = null!;

    [Column("DEPTID")]
    [StringLength(10)]
    [Unicode(false)]
    public string Deptid { get; set; } = null!;

    [Column("DESCR_DEPT")]
    [StringLength(30)]
    [Unicode(false)]
    public string DescrDept { get; set; } = null!;

    [Column("SETID_JOBCODE")]
    [StringLength(5)]
    [Unicode(false)]
    public string SetidJobcode { get; set; } = null!;

    [Column("JOBCODE")]
    [StringLength(6)]
    [Unicode(false)]
    public string Jobcode { get; set; } = null!;

    [Column("JOBCODE_DESCR")]
    [StringLength(30)]
    [Unicode(false)]
    public string JobcodeDescr { get; set; } = null!;

    [Column("GP_PAYGROUP")]
    [StringLength(10)]
    [Unicode(false)]
    public string GpPaygroup { get; set; } = null!;

    [Column("SUPERVISOR_ID")]
    [StringLength(11)]
    [Unicode(false)]
    public string SupervisorId { get; set; } = null!;

    [Column("LOCATION")]
    [StringLength(10)]
    [Unicode(false)]
    public string Location { get; set; } = null!;

    [Column("LOCATION_DESCR")]
    [StringLength(30)]
    [Unicode(false)]
    public string LocationDescr { get; set; } = null!;
}
