using ConfigTransform;
using DemoApp;
using DemoApp.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;


var decryptedConfiguration = new TransformingConfigurationBuilder()
    .AddConfigTransformer("aes", DecryptingConfigTransformer.CreateAesTransformer("key.bin", "iv.bin"))
    .Add(new JsonConfigurationSource {Path = "appsettings.json"})
    .Build();

var serviceProvider = new ServiceCollection().AddSingleton<IConfiguration>(decryptedConfiguration)
    .ConfigureSettings<ApplicationSettings>(decryptedConfiguration)
    .ConfigureSettings<DatabaseSettings>(decryptedConfiguration)
    .AddSingleton<ExampleService>()
    .BuildServiceProvider();

var service = serviceProvider.GetRequiredService<ExampleService>();
service.PrintSettings();
