using System.Security.Cryptography;
using ConfigTransform;
using DemoApp;
using DemoApp.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

// Load AES key and IV from file
var key = File.ReadAllBytes("key.bin");
var iv = File.ReadAllBytes("iv.bin");
var aes = Aes.Create();
aes.Key = key;
aes.IV = iv;
aes.Padding = PaddingMode.PKCS7;
var transformers = new Dictionary<string, IConfigTransformer>
{
    {"aes", DecryptingConfigTransformer.CreateAesTransformer("key.bin", "iv.bin")}
};

var decryptedConfiguration = new TransformingConfigurationBuilder(transformers)
    .Add(new JsonConfigurationSource {Path = "appsettings.json"})
    .Build();

var serviceProvider = new ServiceCollection().AddSingleton<IConfiguration>(decryptedConfiguration)
    .ConfigureSettings<ApplicationSettings>(decryptedConfiguration)
    .ConfigureSettings<DatabaseSettings>(decryptedConfiguration)
    .AddSingleton<ExampleService>()
    .BuildServiceProvider();

var service = serviceProvider.GetRequiredService<ExampleService>();
service.PrintSettings();

var databaseSettings = serviceProvider.GetRequiredService<IOptionsMonitor<DatabaseSettings>>()
    .CurrentValue;
databaseSettings.Server = "newServer";
service.PrintSettings();
