using Microsoft.EntityFrameworkCore;
using AnthologySap.Models;

namespace AnthologySap.Extension;

public static class VUsuariosUnificadoQueryExtensions
{
    // Centralized type/status definitions for Anthology SAP
    private static readonly string[] StudentTypes =
        ["Doctorado", "Especialidad", "Licenciatura", "Maestria", "Preparatoria"];

    // Common role filters
    public static IQueryable<VUsuariosUnificado> WhereEmployeeType(this IQueryable<VUsuariosUnificado> query)
        => query.Where(e => e.AsgmtType == "Empleado");

    public static IQueryable<VUsuariosUnificado> WhereProfessorType(this IQueryable<VUsuariosUnificado> query)
        => query.Where(e => e.AsgmtType == "Profesor");

    public static IQueryable<VUsuariosUnificado> WhereRetiredType(this IQueryable<VUsuariosUnificado> query)
        => query.Where(e => e.AsgmtType == "Jubilado");

    public static IQueryable<VUsuariosUnificado> WhereStudentTypes(this IQueryable<VUsuariosUnificado> query)
        => query.Where(e => EF.Constant(StudentTypes).Contains(e.AsgmtType));

    // Generic status helpers
    public static IQueryable<VUsuariosUnificado> WhereActiveCollaborator(this IQueryable<VUsuariosUnificado> query)
        => query.Where(e => e.StatusField == "A" && e.ProgStatus == "Activo");

    public static IQueryable<VUsuariosUnificado> WhereInactiveCollaborator(this IQueryable<VUsuariosUnificado> query)
        => query.Where(e => e.StatusField == "I" && e.ProgStatus == "Inactivo");

    // Students logic
    public static IQueryable<VUsuariosUnificado> WhereActiveStudents(this IQueryable<VUsuariosUnificado> query)
        => query.Where(e =>
            (
                (e.ProgStatus == "Activo" && e.StatusField == "ATT")
                || (e.ProgStatus == "Matriculado" && e.StatusField == "FUT")
            )
        );

    public static IQueryable<VUsuariosUnificado> WhereInactiveStudents(this IQueryable<VUsuariosUnificado> query)
        => query.Where(e =>
            (
                (e.ProgStatus == "Inactivo" && e.StatusField == "DROP")
                || (e.ProgStatus == "Baja" && e.StatusField == "NDSBAJ")
            )
        );

    public static IQueryable<VUsuariosUnificado> WhereGraduatedStudents(this IQueryable<VUsuariosUnificado> query)
        => query.Where(e =>
            (
                (e.ProgStatus == "Titulado" && e.StatusField == "GRAD")
                || (e.ProgStatus == "Egresado" && e.StatusField == "COMPLETE")
            )
        );

    // Cross-category exclusion used by AnthologySap Inactive repositories
    // Note: name mirrors UP.Data counterpart semantics (excludes any active match)
    public static IQueryable<VUsuariosUnificado> WhereAnyActiveProfile(
        this IQueryable<VUsuariosUnificado> query,
        AppDbContext context
    )
    {
        return query.Where(i => !context.VUsuariosUnificados
            .Where(a =>
                (a.Emplid.Length > 7 ? a.Emplid.Substring(a.Emplid.Length - 7, 7) : a.Emplid)
                == i.Emplid
            )
            .Any(a =>
                // Active employees
                (a.StatusField == "A" && a.ProgStatus == "Activo" && a.AsgmtType == "Empleado")
                // Active professors
                || (a.StatusField == "A" && a.ProgStatus == "Activo" && a.AsgmtType == "Profesor")
                // Active students
                || (
                    (a.ProgStatus == "Activo" && a.StatusField == "ATT")
                    || (a.ProgStatus == "Matriculado" && a.StatusField == "FUT")
                ) && EF.Constant(StudentTypes).Contains(a.AsgmtType)
                // Graduated
                || (
                    (a.ProgStatus == "Titulado" && a.StatusField == "GRAD")
                    || (a.ProgStatus == "Egresado" && a.StatusField == "COMPLETE")
                ) && EF.Constant(StudentTypes).Contains(a.AsgmtType)
                // Retired employees
                || (a.StatusField == "A" && a.ProgStatus == "Activo" && a.AsgmtType == "Jubilado")
            ));
    }
}
