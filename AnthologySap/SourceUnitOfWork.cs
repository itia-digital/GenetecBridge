using AnthologySap.Models;
using AnthologySap.Repositories;
using Core.Data;
using Core.Data.Repositories;

namespace AnthologySap;

// ReSharper disable once InconsistentNaming
public class SourceUnitOfWork(AppDbContext context) : ISourceUnitOfWork
{
    private IActiveEmployeesRepository? _activeEmployees;

    private IActiveProfessorsRepository? _activeProfessors;

    private IActiveStudentsRepository? _activeStudents;

    private IGraduatedRepository? _graduated;

    private IInactiveEmployeesRepository? _inactiveEmployees;

    private IInactiveProfessorsRepository? _inactiveProfessors;

    private IInactiveStudentsRepository? _inactiveStudents;

    private IStatusRepository? _allRecords;

    private IRetiredRepository? _retiredEmployees;

    private UtilitiesRepository? _utilities;

    public void Dispose()
    {
        context.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await context.DisposeAsync();
    }

    public IActiveEmployeesRepository ActiveEmployees =>
        _activeEmployees ??= new ActiveEmployeesRepository(context);

    public IActiveProfessorsRepository ActiveProfessors =>
        _activeProfessors ??= new ActiveProfessorsRepository(context);

    public IActiveStudentsRepository ActiveStudents =>
        _activeStudents ??= new ActiveStudentsRepository(context);

    public IGraduatedRepository Graduated =>
        _graduated ??= new GraduatedRepository(context);

    public IInactiveEmployeesRepository InactiveEmployees =>
        _inactiveEmployees ??= new InactiveEmployeesRepository(context);

    public IInactiveProfessorsRepository InactiveProfessors =>
        _inactiveProfessors ??= new InactiveProfessorsRepository(context);

    public IInactiveStudentsRepository InactiveStudents =>
        _inactiveStudents ??= new InactiveStudentsRepository(context);

    public IStatusRepository Status =>
        _allRecords ??= new StatusRepository(context);

    public IRetiredRepository RetiredEmployees =>
        _retiredEmployees ??= new RetiredEmployeesRepository(context);

    public IUtilitiesRepository Utilities =>
        _utilities ??= new UtilitiesRepository(context);
}