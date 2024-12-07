using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace Core.Data.Extensions;

public static class ModelBuilderExtensions
{
    public static void ApplyAllConfigurations(this ModelBuilder modelBuilder, Assembly assembly)
    {
        // Get all types implementing IEntityTypeConfiguration<T>
        var configurations = assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false })
            .SelectMany(t => t.GetInterfaces()
                .Where(i => i.IsGenericType &&
                            i.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>))
                .Select(i => new { EntityType = i.GenericTypeArguments[0], ConfigType = t }))
            .ToList();

        foreach (var config in configurations)
        {
            // Create an instance of the configuration class
            var configurationInstance = Activator.CreateInstance(config.ConfigType);

            // Apply the configuration
            var method = typeof(ModelBuilder)
                .GetMethod(nameof(ModelBuilder.ApplyConfiguration), BindingFlags.Instance | BindingFlags.Public)
                ?.MakeGenericMethod(config.EntityType);

            method?.Invoke(modelBuilder, [configurationInstance]);
        }
    }
}