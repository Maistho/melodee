using System.Text.Json.Serialization;

namespace Melodee.Common.Plugins.SearchEngine.WikiData;

/// <summary>
///     WikiData SPARQL query result DTOs.
/// </summary>
public class WikiDataSparqlResult
{
    [JsonPropertyName("head")]
    public WikiDataHead? Head { get; set; }

    [JsonPropertyName("results")]
    public WikiDataResults? Results { get; set; }
}

public class WikiDataHead
{
    [JsonPropertyName("vars")]
    public List<string>? Vars { get; set; }
}

public class WikiDataResults
{
    [JsonPropertyName("bindings")]
    public List<WikiDataBinding>? Bindings { get; set; }
}

public class WikiDataBinding
{
    [JsonPropertyName("item")]
    public WikiDataValue? Item { get; set; }

    [JsonPropertyName("itemLabel")]
    public WikiDataValue? ItemLabel { get; set; }

    [JsonPropertyName("description")]
    public WikiDataValue? Description { get; set; }

    [JsonPropertyName("image")]
    public WikiDataValue? Image { get; set; }
}

public class WikiDataValue
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("value")]
    public string? Value { get; set; }
}
