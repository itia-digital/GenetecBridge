using Microsoft.EntityFrameworkCore;
using UP.Data.Context;
using UP.Data.Models;

namespace UP.Data.Extension;

public static class PsUpIdGralTVwQueryExtensions
{
    // Centralized pay group definitions
    private static readonly string[] EmployeePayGroups =
        ["UPA001", "UPC001", "UPE001", "UPG001", "UPM001"];

    private static readonly string[] ProfessorPayGroups =
        ["UPAA001", "UPGA001", "UPMA001", "UPAH001", "UPGH001", "UPMH001"];

    private static readonly string[] RetiredPayGroups = 
        ["UPAP001", "UPGP001", "UPMP001"];

    private static readonly string[] InactiveStudentStatuses = 
        ["CN", "DC", "DE", "LA"];

    // Simple pay group filters
    public static IQueryable<PsUpIdGralTVw> WhereEmployeePayGroup(this IQueryable<PsUpIdGralTVw> query)
        => query.Where(e => EF.Constant(EmployeePayGroups).Contains(e.GpPaygroup));

    public static IQueryable<PsUpIdGralTVw> WhereProfessorPayGroup(this IQueryable<PsUpIdGralTVw> query)
        => query.Where(e => EF.Constant(ProfessorPayGroups).Contains(e.GpPaygroup));

    public static IQueryable<PsUpIdGralTVw> WhereRetiredPayGroup(this IQueryable<PsUpIdGralTVw> query)
        => query.Where(e => EF.Constant(RetiredPayGroups).Contains(e.GpPaygroup));

    public static IQueryable<PsUpIdGralTVw> WhereActiveStudent(this IQueryable<PsUpIdGralTVw> query)
        => query.Where(e => e.StatusField == "AC");

    public static IQueryable<PsUpIdGralTVw> WhereActiveCollaborator(this IQueryable<PsUpIdGralTVw> query)
        => query.Where(e => e.StatusField == "A");

    public static IQueryable<PsUpIdGralTVw> WhereInactiveCollaborator(this IQueryable<PsUpIdGralTVw> query)
        => query.Where(e => e.StatusField == "I");

    public static IQueryable<PsUpIdGralTVw> WhereInactiveStudentStatuses(this IQueryable<PsUpIdGralTVw> query)
        => query.Where(e => EF.Constant(InactiveStudentStatuses).Contains(e.StatusField));

    // Cross-category exclusion used by Inactive repositories
    public static IQueryable<PsUpIdGralTVw> WhereAnyActiveProfile(
        this IQueryable<PsUpIdGralTVw> query,
        AppDbContext context
    )
    {
        return query.Where(i => !context.PsUpIdGralTVws
            .Where(a => a.Emplid == i.Emplid)
            .Any(a =>
                // Active employees
                (a.StatusField == "A" && EF.Constant(EmployeePayGroups).Contains(a.GpPaygroup))
                // Active professors
                || (a.StatusField == "A" && EF.Constant(ProfessorPayGroups).Contains(a.GpPaygroup))
                // Active students
                || a.StatusField == "AC"
                // Graduated
                || (a.StatusField == "DM" && a.ProgReason == "EGR")
                || (a.StatusField == "CM" && a.ProgReason == "CRED")
                || (a.StatusField == "SP" && (a.ProgReason == "EGR" || a.ProgReason == "EGRP"))
            ));
    }
}