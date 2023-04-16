# ConfigTransform

A C# class library for transforming configuration sources at runtime.

## Usage

See the Demo App for an example of how to use this library.

Generally, you will want to implement `IConfigTransformer`
and pass them to a `TransformingConfigurationBuilder`. The constructor takes a dictionary
of `(string, IConfigurationTransformer)`-typed entries. In your configuration, values of the form `${key:value}` will be
transformed by the corresponding `IConfigTransformer` instance.

For example, the demo app uses the included `DecryptingConfigTransformer` to decrypt AES-encrypted values in the configuration.
In the config, the value is prefixed with `aes:` and the encrypted value is base64-encoded:

```json
{
  "Database": {
    "User": {
      "password": "${aes:kUji1MHTY8lY6yFd4PQUig==}"
    }
  }
}
```

The `DecryptingConfigTransformer` is configured with a key and IV file, which are used to decrypt the value:

```csharp
var transformers = new Dictionary<string, IConfigTransformer>
{
    {"aes", DecryptingConfigTransformer.CreateAesTransformer("key.bin", "iv.bin")}
};

var decryptedConfiguration = new TransformingConfigurationBuilder(transformers)
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
