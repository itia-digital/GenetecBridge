using UP.Data.Context;
using UP.Data.Repositories;

namespace UP.Data;

// ReSharper disable once InconsistentNaming
public interface IUpUnitOfWork : IDisposable, IAsyncDisposable
{
    IActiveEmployeesRepository ActiveEmployees { get; }
    IActiveProfessorsRepository ActiveProfessors { get; }
    IActiveStudentsRepository ActiveStudents { get; }
    IApplicantsRepositoryRepository Applicants { get; }
    IGraduatedStudentRepository Graduated { get; }
    IInactiveEmployeesRepository InactiveEmployees { get; }
    IInactiveStudentsRepository InactiveStudents { get; }
}

public class UpUnitOfWork(UpDbContext context) : IUpUnitOfWork
{
    public void Dispose()
    {
        context.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await context.DisposeAsync();
    }

    private IActiveEmployeesRepository? _graduates;

    public IActiveEmployeesRepository ActiveEmployees =>
        _graduates ??= new ActiveEmployeesRepository(context);

    
    private IActiveProfessorsRepository? _activeProfessors;
    public IActiveProfessorsRepository ActiveProfessors =>
        _activeProfessors ??= new ActiveProfessorsRepository(context);
    
    private IActiveStudentsRepository? _activeStudents;
    public IActiveStudentsRepository ActiveStudents =>
        _activeStudents ??= new ActiveStudentsRepository(context);
    
    private IApplicantsRepositoryRepository? _applicants;
    public IApplicantsRepositoryRepository Applicants =>
        _applicants ??= new ApplicantsRepositoryRepository(context);
    
    private IGraduatedStudentRepository? _graduated;
    public IGraduatedStudentRepository Graduated =>
        _graduated ??= new GraduatedStudentRepository(context);
    
    private IInactiveEmployeesRepository? _inactiveEmployees;
    public IInactiveEmployeesRepository InactiveEmployees =>
        _inactiveEmployees ??= new InactiveEmployeesRepository(context);
    
    private IInactiveStudentsRepository? _inactiveStudents;
    public IInactiveStudentsRepository InactiveStudents =>
    _inactiveStudents ??= new InactiveStudentsRepository(context);
}
