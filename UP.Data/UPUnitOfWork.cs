using UP.Data.Context;

namespace UP.Data;

// ReSharper disable once InconsistentNaming
public interface IUPUnitOfWork : IDisposable
{
    IGraduatesRepository Graduates { get; }
    IStudentsRepository Students { get; }
}

public class UPUnitOfWork(UpDbContext context) : IUPUnitOfWork
{
    private IGraduatesRepository? _graduates;
    private IStudentsRepository? _students;

    public IGraduatesRepository Graduates =>
        _graduates ??= new GraduatesRepository(context);

    public IStudentsRepository Students =>
        _students ??= new StudentsesRepository(context);

    public void Dispose()
    {
        context.Dispose();
    }
}