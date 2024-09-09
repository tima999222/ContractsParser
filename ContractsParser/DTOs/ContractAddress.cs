using System.Text.Json.Serialization;

namespace ContractsParser.DTOs
{
    public class ContractAddress
    {
        [JsonPropertyName("hash")]
        public string Hash { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}
