namespace ConfigTransform;

public interface IConfigTransformer
{
    string? Transform(string? value);
    string? ReverseTransform(string? value);
}
