using UP.Data.Context;
using UP.Data.Repositories;

namespace UP.Data;

// ReSharper disable once InconsistentNaming
public interface IUpUnitOfWork : IDisposable, IAsyncDisposable
{
    IActiveEmployeesRepository ActiveEmployees { get; }
    IActiveProfessorsRepository ActiveProfessors { get; }
    IActiveStudentsRepository ActiveStudents { get; }
    IGraduatedStudentRepository Graduated { get; }
    IInactiveEmployeesRepository InactiveEmployees { get; }
    IRetiredEmployeesRepository RetiredEmployees { get; }
    IInactiveStudentsRepository InactiveStudents { get; }
}

public class UpUnitOfWork(UpDbContext context) : IUpUnitOfWork
{
    private IActiveProfessorsRepository? _activeProfessors;

    private IActiveStudentsRepository? _activeStudents;

    private IGraduatedStudentRepository? _graduated;

    private IActiveEmployeesRepository? _graduates;

    private IRetiredEmployeesRepository? _retiredEmployees;

    private IInactiveEmployeesRepository? _inactiveEmployees;
    
    private IInactiveStudentsRepository? _inactiveStudents;

    public void Dispose()
    {
        context.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await context.DisposeAsync();
    }

    public IActiveEmployeesRepository ActiveEmployees =>
        _graduates ??= new ActiveEmployeesRepository(context);

    public IActiveProfessorsRepository ActiveProfessors =>
        _activeProfessors ??= new ActiveProfessorsRepository(context);

    public IActiveStudentsRepository ActiveStudents =>
        _activeStudents ??= new ActiveStudentsRepository(context);

    public IGraduatedStudentRepository Graduated =>
        _graduated ??= new GraduatedStudentRepository(context);

    public IRetiredEmployeesRepository RetiredEmployees =>
        _retiredEmployees ??= new RetiredEmployeesRepository(context);

    public IInactiveEmployeesRepository InactiveEmployees =>
        _inactiveEmployees ??= new InactiveEmployeesRepository(context);

    public IInactiveStudentsRepository InactiveStudents =>
        _inactiveStudents ??= new InactiveStudentsRepository(context);
}