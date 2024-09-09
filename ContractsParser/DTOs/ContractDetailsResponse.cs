using System.Text.Json.Serialization;

namespace ContractsParser.DTOs
{
    public class ContractDetailsResponse
    {
        [JsonPropertyName("source_code")]
        public string SourceCode { get; set; }
    }
}
