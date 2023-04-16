using System.Security.Cryptography;
using System.Text;
using ConfigTransform.Transformers;
using Microsoft.Extensions.Configuration.Memory;
using Xunit.Abstractions;

namespace ConfigTransform.Tests;

public class DecryptingConfigTransformerTests
{
    [Theory]
    [InlineData(TestValue, PaddingMode.PKCS7)]
    [InlineData(TestValue, PaddingMode.ANSIX923)]
    [InlineData(TestValue, PaddingMode.ISO10126)]
    [InlineData(TestValue, PaddingMode.Zeros)]
    public void TestEncrypt(string testValue, PaddingMode paddingMode)
    {
        using var aes = Aes.Create();
        var decryptingConfigTransformer
            = DecryptingConfigTransformer.CreateAesTransformer(aes.Key, aes.IV, paddingMode);

        var encrypted = decryptingConfigTransformer.Encrypt(testValue);
        _output.WriteLine($"Unencrypted: {testValue}");
        _output.WriteLine($"Encrypted: {encrypted}");
        Assert.NotNull(encrypted);
        Assert.NotEqual(testValue, encrypted);
        var bytes = Convert.FromBase64String(encrypted);
        Assert.NotEmpty(bytes);
    }

    [Theory]
    [InlineData(TestValue, PaddingMode.PKCS7)]
    [InlineData(TestValue, PaddingMode.ANSIX923)]
    [InlineData(TestValue, PaddingMode.ISO10126)]
    [InlineData(TestValue, PaddingMode.Zeros)]
    public void TestEncryptDecrypt(string testValue, PaddingMode paddingMode)
    {
        using var aes = Aes.Create();
        var decryptingConfigTransformer
            = DecryptingConfigTransformer.CreateAesTransformer(aes.Key, aes.IV, paddingMode);

        _output.WriteLine($"Unencrypted: {testValue}");
        var encrypted = decryptingConfigTransformer.Encrypt(testValue);
        _output.WriteLine($"Encrypted: {encrypted}");
        var decrypted = decryptingConfigTransformer.Decrypt(encrypted);
        _output.WriteLine($"Decrypted: {decrypted}");
    }

    [Theory]
    [InlineData(PaddingMode.PKCS7)]
    [InlineData(PaddingMode.ANSIX923)]
    [InlineData(PaddingMode.ISO10126)]
    [InlineData(PaddingMode.Zeros)]
    public void TestBuildConfigWithTransformer(PaddingMode paddingMode)
    {
        using var aes = Aes.Create();
        aes.Padding = paddingMode;
        const string key = "test";
        const string value = "value";
        _output.WriteLine($"Unencrypted: {value}");
        var encrypted = Convert.ToBase64String(
            aes.CreateEncryptor()
                .TransformFinalBlock(Encoding.UTF8.GetBytes(value), 0, value.Length));

        _output.WriteLine($"Encrypted: {encrypted}");
        var valuePair = $"${{{key}:{encrypted}}}";
        var transformers = new Dictionary<string, IConfigTransformer>
        {
            {
                key,
                DecryptingConfigTransformer.CreateAesTransformer(aes.Key, aes.IV, paddingMode)
            }
        };

        var source = new MemoryConfigurationSource
        {
            InitialData = new[] {new KeyValuePair<string, string>(key, valuePair)}!
        };

        var builder = new TransformingConfigurationBuilder(transformers);
        builder.Add(source);
        var config = builder.Build();
        var decrypted = config[key];
        _output.WriteLine($"Decrypted: {decrypted}");
        Assert.NotNull(config);
        Assert.Equal(value, decrypted);
    }

    private const string TestValue = "Hello, world!";
    private readonly ITestOutputHelper _output;

    public DecryptingConfigTransformerTests(ITestOutputHelper output)
    {
        _output = output;
    }
}
