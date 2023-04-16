using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace ConfigTransform;

public partial class TransformingConfigurationProvider : IConfigurationProvider
{
    public bool TryGet(string key, out string? value)
    {
        if (_transformedConfiguration.TryGetValue(key, out value)) return true;
        if (!_configuration.TryGet(key, out value)) return false;
        if (!TryParseKeyValuePair(value, out var innerKey, out var innerValue)) return true;
        value = TransformForward(innerValue, innerKey);
        _transformedConfiguration[key] = value;
        return true;
    }

    public void Set(string key, string? value)
    {
        _transformedConfiguration[key] = value;
        if (TryParseKeyValuePair(value, out var transformerKind, out _))
        {
            value = TransformBackward(value, transformerKind);
        }

        var keyValuePairString = GetTransformedKeyValuePair(key, value);
        _configuration.Set(key, keyValuePairString);
    }

    public IChangeToken GetReloadToken()
    {
        return _configuration.GetReloadToken();
    }

    public void Load()
    {
        _configuration.Load();
        _transformedConfiguration.Clear();
    }

    public IEnumerable<string> GetChildKeys(IEnumerable<string> earlierKeys, string? parentPath)
    {
        return _configuration.GetChildKeys(earlierKeys, parentPath);
    }

    [GeneratedRegex(
        @"\$\{(?<key>\w+):(?<value>[^}]+)\}",
        RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private static partial Regex EncryptedKeyValuePairRegex();

    private static string GetTransformedKeyValuePair(string key, string? value) =>
        $"${{{key}:{value}}}";

    private static bool TryParseKeyValuePair(string? str, out string? key, out string? value)
    {
        if (str == null)
        {
            key = null;
            value = null;
            return false;
        }

        var match = EncryptedKeyValuePairRegex().Match(str);
        if (!match.Success)
        {
            key = null;
            value = null;
            return false;
        }

        key = match.Groups["key"].Value;
        value = match.Groups["value"].Value;
        return true;
    }

    private string? TransformForward(string? value, string? transformerKind)
    {
        if (transformerKind == null) return value;
        if (!_transformers.TryGetValue(transformerKind, out var transformer))
            throw new NotSupportedException($"Unsupported config transform: {transformerKind}");

        return transformer.Transform(value);
    }

    private string? TransformBackward(string? value, string? transformerKind)
    {
        if (transformerKind == null) return value;
        if (!_transformers.TryGetValue(transformerKind, out var transformer))
            throw new NotSupportedException($"Unsupported config transform: {transformerKind}");

        return transformer.ReverseTransform(value);
    }

    private readonly IConfigurationProvider _configuration;
    private readonly Dictionary<string, string?> _transformedConfiguration = new();
    private readonly IDictionary<string, IConfigTransformer> _transformers;

    public TransformingConfigurationProvider(
        IConfigurationProvider configuration,
        IDictionary<string, IConfigTransformer> transformers)
    {
        _configuration = configuration;
        _transformers = transformers;
    }
}
