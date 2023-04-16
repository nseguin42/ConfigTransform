using Microsoft.Extensions.Configuration;

namespace ConfigTransform;

public class TransformedConfigurationSource : IConfigurationSource
{
    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        if (builder is not TransformingConfigurationBuilder transformingBuilder)
            throw new ArgumentException(
                $"Builder must be of type {nameof(TransformingConfigurationBuilder)}",
                nameof(builder));

        return Build(transformingBuilder);
    }

    private TransformingConfigurationProvider Build(TransformingConfigurationBuilder builder)
    {
        return new TransformingConfigurationProvider(
            _configuration.Build(builder),
            builder.ConfigTransformers);
    }

    private readonly IConfigurationSource _configuration;

    internal TransformedConfigurationSource(IConfigurationSource config)
    {
        _configuration = config;
    }
}
