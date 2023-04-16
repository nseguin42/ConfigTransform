# ConfigTransform

A C# class library for transforming configuration sources at runtime.

## Usage

See the Demo App for an example of how to use this library.

The `TransformingConfigurationBuilder` class is used to build a configuration provider that applies transformations
to values of the form `${key:value}`. The transformations are implemented by instances of `IConfigTransformer`.

For example, the demo app uses the included `DecryptingConfigTransformer` to decrypt AES-encrypted values in the
configuration.

```json
{
  "Database": {
    "User": {
      "password": "${aes:kUji1MHTY8lY6yFd4PQUig==}"
    }
  }
}
```

```csharp
var decryptedConfiguration = new TransformingConfigurationBuilder(transformers)
    .AddConfigTransformer("aes", DecryptingConfigTransformer.CreateAesTransformer("key.bin", "iv.bin"))
    .Add(new JsonConfigurationSource {Path = "appsettings.json"})
    .Build();

var serviceProvider = new ServiceCollection().AddSingleton<IConfiguration>(decryptedConfiguration)
    .ConfigureSettings<DatabaseSettings>(decryptedConfiguration)
    ...
    .BuildServiceProvider();

var databaseSettings = serviceProvider.GetService<DatabaseSettings>();
Console.WriteLine($"Password: \"{databaseSettings.User.Password}\"");
```

```output
Password: "TEST123"
```
