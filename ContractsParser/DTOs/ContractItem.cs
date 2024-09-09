using System.Text.Json.Serialization;

namespace ContractsParser.DTOs
{
    public class ContractItem
    {
        [JsonPropertyName("address")]
        public ContractAddress Address { get; set; }

        [JsonPropertyName("tx_count")]
        public int? TxCount { get; set; }
    }
}
