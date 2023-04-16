using Microsoft.Extensions.Configuration;

namespace ConfigTransform;

public class TransformingConfigurationBuilder : ConfigurationBuilder
{
    public IDictionary<string, IConfigTransformer> ConfigTransformers { get; }

    public new IConfigurationBuilder Add(IConfigurationSource source)
    {
        if (source is not TransformedConfigurationSource)
            source = new TransformedConfigurationSource(source);

        return base.Add(source);
    }

    public TransformingConfigurationBuilder(
        IDictionary<string, IConfigTransformer> configTransformers)
    {
        ConfigTransformers = configTransformers;
    }

    public new IConfigurationRoot Build()
    {
        return base.Build();
    }
}
