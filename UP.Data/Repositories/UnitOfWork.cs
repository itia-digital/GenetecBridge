using UP.Services.Context;

namespace UP.Services.Repositories;

public interface IUnitOfWork : IDisposable
{
    StudentsRepository Students { get; }
}

public class UnitOfWork(UpDbContext context) : IUnitOfWork
{
    private StudentsRepository? _students;

    public StudentsRepository Students 
        => _students ??= new StudentsRepository(context);

    public void Dispose()
    {
        context.Dispose();
    }
}