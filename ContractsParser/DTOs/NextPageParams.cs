using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ContractsParser.DTOs
{
    public class NextPageParams
    {
        [JsonPropertyName("coin_balance")]
        public string? CoinBalance { get; set; }

        [JsonPropertyName("items_count")]
        public long? ItemsCount { get; set; }

        [JsonPropertyName("smart_contract_id")]
        public long? SmartContractId { get; set; }

        [JsonPropertyName("tx_count")]
        public long? TxCount { get; set; }   
    }
}
