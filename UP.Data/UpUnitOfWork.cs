using UP.Data.Context;
using UP.Data.Repositories;

namespace UP.Data;

// ReSharper disable once InconsistentNaming
public interface IUpUnitOfWork : IDisposable, IAsyncDisposable
{
    IActiveEmployeesRepository ActiveEmployees { get; }
    IActiveProfessorsRepository ActiveProfessors { get; }
    IActiveStudentsRepository ActiveStudents { get; }
    IGraduatedRepository Graduated { get; }
    IInactiveEmployeesRepository InactiveEmployees { get; }
    IInactiveProfessorsRepository InactiveProfessors { get; }
    IInactiveStudentsRepository InactiveStudents { get; }
    IRetiredRepository RetiredEmployees { get; }
}

public class UpUnitOfWork(UpDbContext context) : IUpUnitOfWork
{
    private IActiveEmployeesRepository? _activeEmployees;
    
    private IActiveProfessorsRepository? _activeProfessors;

    private IActiveStudentsRepository? _activeStudents;

    private IGraduatedRepository? _graduated;

    private IInactiveEmployeesRepository? _inactiveEmployees;

    private IInactiveProfessorsRepository? _inactiveProfessors;
    
    private IInactiveStudentsRepository? _inactiveStudents;

    private IRetiredRepository? _retiredEmployees;

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

    public IRetiredRepository RetiredEmployees =>
        _retiredEmployees ??= new RetiredEmployeesRepository(context);
}