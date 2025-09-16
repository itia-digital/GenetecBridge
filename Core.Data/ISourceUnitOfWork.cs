using Core.Data.Repositories;

namespace Core.Data;

public interface ISourceUnitOfWork : IDisposable, IAsyncDisposable
{
    IActiveEmployeesRepository ActiveEmployees { get; }
    IActiveProfessorsRepository ActiveProfessors { get; }
    IActiveStudentsRepository ActiveStudents { get; }
    IGraduatedRepository Graduated { get; }
    IInactiveEmployeesRepository InactiveEmployees { get; }
    IInactiveProfessorsRepository InactiveProfessors { get; }
    IInactiveStudentsRepository InactiveStudents { get; }
    IStatusRepository Status { get; }
    IRetiredRepository RetiredEmployees { get; }
    IUtilitiesRepository Utilities { get; }
}