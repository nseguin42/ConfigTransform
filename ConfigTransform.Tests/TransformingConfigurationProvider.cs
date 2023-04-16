using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Xunit.Abstractions;

namespace ConfigTransform.Tests;

public class ConfigTransformerTests
{
    public ConfigTransformerTests(ITestOutputHelper output)
    {
        _output = output;
    }

    private readonly ITestOutputHelper _output;

    [Fact]
    public void TestBuildConfigWithNoTransformers()
    {
        var config
            = new TransformingConfigurationBuilder(new Dictionary<string, IConfigTransformer>())
                .Add(
                    new MemoryConfigurationSource
                        {InitialData = new[] {new KeyValuePair<string, string>("key", "value")}!})
                .Build();

        Assert.NotNull(config);
        Assert.Equal("value", config["key"]);
    }

    [Fact]
    public void TestBuildConfigWithTransformers()
    {
        var value = "value";
        var config = new TransformingConfigurationBuilder(
                new Dictionary<string, IConfigTransformer>
                {
                    ["test"] = new TestConfigTransformer()
                }).Add(
                new MemoryConfigurationSource
                {
                    InitialData = new[]
                        {new KeyValuePair<string, string>("key", $"\\${{test:{value}}}")}!
                })
            .Build();

        Assert.NotNull(config);
        Assert.Equal($"transformed({value})", config["key"]);
    }

    private class TestConfigTransformer : IConfigTransformer
    {
        public string Transform(string? value)
        {
            return $"transformed({value})";
        }

        public string ReverseTransform(string? value)
        {
            throw new NotSupportedException();
        }
    }
}
