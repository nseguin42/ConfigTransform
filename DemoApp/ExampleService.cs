using DemoApp.Settings;
using Microsoft.Extensions.Options;

namespace DemoApp;

public class ExampleService
{
    public void PrintSettings()
    {
        Console.WriteLine($"Application version: {_applicationSettings.Version}");
        Console.WriteLine($"Database server: {_databaseSettings.Server}");
        Console.WriteLine($"Database name: {_databaseSettings.Database}");
        Console.WriteLine($"Database user: {_databaseSettings.User?.Username}");
        Console.WriteLine($"Database password: {_databaseSettings.User?.Password}");
    }

    private ApplicationSettings _applicationSettings;
    private DatabaseSettings _databaseSettings;

    public ExampleService(
        IOptionsMonitor<ApplicationSettings> applicationSettings,
        IOptionsMonitor<DatabaseSettings> databaseSettings)
    {
        _applicationSettings = applicationSettings.CurrentValue;
        _databaseSettings = databaseSettings.CurrentValue;
        applicationSettings.OnChange(settings => _applicationSettings = settings);
        databaseSettings.OnChange(settings => _databaseSettings = settings);
    }
}
