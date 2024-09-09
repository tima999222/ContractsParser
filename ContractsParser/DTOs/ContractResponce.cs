using System.Text.Json.Serialization;

namespace ContractsParser.DTOs
{
    public class ContractResponse
    {
        [JsonPropertyName("items")]
        public List<ContractItem> Items { get; set; }

        [JsonPropertyName("next_page_params")]
        public NextPageParams NextPageParams { get; set; }
    }
}
