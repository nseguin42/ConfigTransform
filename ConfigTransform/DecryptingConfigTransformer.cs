using System.Security.Cryptography;

namespace ConfigTransform;

public class DecryptingConfigTransformer : IConfigTransformer
{
    public static DecryptingConfigTransformer CreateAesTransformer(
        byte[] key,
        byte[] iv,
        PaddingMode paddingMode = PaddingMode.PKCS7)
    {
        var aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;
        aes.Padding = paddingMode;
        return new DecryptingConfigTransformer(aes.CreateDecryptor(), aes.CreateEncryptor());
    }

    public static DecryptingConfigTransformer CreateAesTransformer(
        string pathToKeyFile,
        string pathToIvFile)
    {
        var key = File.ReadAllBytes(pathToKeyFile);
        var iv = File.ReadAllBytes(pathToIvFile);
        return CreateAesTransformer(key, iv);
    }

    public string? Transform(string? value)
    {
        TryDecrypt(value, out var result);
        return result;
    }

    public string? ReverseTransform(string? value)
    {
        return Encrypt(value);
    }

    internal string? Encrypt(string? value)
    {
        if (value == null) return null;
        if (_encrypt == null) throw new InvalidOperationException("No encryptor available");
        using var ms = new MemoryStream();
        using (var cs = new CryptoStream(ms, _encrypt, CryptoStreamMode.Write))
        {
            using (var sw = new StreamWriter(cs))
            {
                sw.Write(value);
            }
        }

        var encrypted = ms.ToArray();
        return Convert.ToBase64String(encrypted);
    }

    internal string? Decrypt(string? value)
    {
        if (value == null) return null;
        var bytes = Convert.FromBase64String(value);
        using var ms = new MemoryStream(bytes);
        using var cs = new CryptoStream(ms, _decrypt, CryptoStreamMode.Read);
        using var sw = new StreamReader(cs);
        return sw.ReadToEnd().TrimEnd('\0');
    }

    private bool TryDecrypt(string? value, out string? result)
    {
        try
        {
            result = Decrypt(value);
            return true;
        }
        catch (Exception)
        {
            result = null;
            return false;
        }
    }

    public DecryptingConfigTransformer(ICryptoTransform decrypt, ICryptoTransform? encrypt)
    {
        _decrypt = decrypt;
        _encrypt = encrypt;
    }

    private readonly ICryptoTransform _decrypt;
    private readonly ICryptoTransform? _encrypt;
}
