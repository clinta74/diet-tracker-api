using System.Text.RegularExpressions;

public partial class SlugifyParameterTransformer : IOutboundParameterTransformer
{
    public string TransformOutbound(object value)
    {
        // Slugify value
        return value == null ? null : CamelCaseBoundary().Replace(value.ToString(), "$1-$2").ToLower();
    }

    [GeneratedRegex("([a-z])([A-Z])")]
    private static partial Regex CamelCaseBoundary();
}
