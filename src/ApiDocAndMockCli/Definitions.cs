using System.Text.Json.Serialization;


namespace ApiDocAndMockCli
{

    public class SchemaDefinition
    {
        [JsonPropertyName("entities")]
        public List<EntityDefinition> Entities { get; set; } = new();
    }

    public class EntityDefinition
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("properties")]
        public Dictionary<string, string> Properties { get; set; } = new();
    }
}
