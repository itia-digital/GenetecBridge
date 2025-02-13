using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GenetecBridgeTester;

public static class Utils
{
    public static ILogger<T> GetLogger<T>()
    {
        var serviceProvider = new ServiceCollection()
            .AddLogging()
            .BuildServiceProvider();

        var factory = serviceProvider.GetRequiredService<ILoggerFactory>();
        return factory.CreateLogger<T>();
    }
    
}