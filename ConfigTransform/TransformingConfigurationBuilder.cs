using Microsoft.Extensions.Configuration;

namespace ConfigTransform;

public class TransformingConfigurationBuilder : ConfigurationBuilder
{
    public IDictionary<string, IConfigTransformer> ConfigTransformers { get; }

    public new IConfigurationBuilder Add(IConfigurationSource source)
    {
        if (source is not TransformingConfigurationSource)
            source = new TransformingConfigurationSource(source);

        return base.Add(source);
    }

    public TransformingConfigurationBuilder() : this(new Dictionary<string, IConfigTransformer>())
    {
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

    public TransformingConfigurationBuilder AddConfigTransformer(
        string name,
        IConfigTransformer transformer)
    {
        ConfigTransformers.Add(name, transformer);
        return this;
    }
}
