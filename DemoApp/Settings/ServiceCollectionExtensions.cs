using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DemoApp.Settings;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection ConfigureOptions<T>(
        this IServiceCollection services,
        IConfiguration configuration,
        string? sectionName = null) where T : class
    {
        return services.Configure(Bind<T>(configuration, sectionName));
    }

    private static Action<T> Bind<T>(IConfiguration configuration, string? sectionName = null)
    {
        if (string.IsNullOrEmpty(sectionName))
        {
            var name = typeof(T).Name;
            sectionName = name.EndsWith(SettingsSuffix) ? name[..^SettingsSuffix.Length] : name;
        }

        var section = configuration.GetSection(sectionName);
        return settings => section.Bind(settings);
    }

    private const string SettingsSuffix = "Settings";
}
