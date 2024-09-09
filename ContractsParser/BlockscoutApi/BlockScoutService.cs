using ContractsParser.DTOs;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace ContractsParser.BlockscoutApi
{
    public class BlockScoutService : IBlockScoutService
    {
        private readonly string _baseUrl = "https://eth.blockscout.com/api/v2/"; //smart-contracts?filter=solidity
        private readonly HttpClient _httpClient;

        public BlockScoutService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<ContractItem>> GetContracts(int count, string q = "")
        {
            string url = _urlBuild($"/smart-contracts?q={q}&filter=solidity");
            var contracts = new List<ContractItem>();

            while (contracts.Count < count)
            {
                // Выполняем запрос к API
                var response = await _httpClient.GetStringAsync(url);
                var contractResponse = JsonSerializer.Deserialize<ContractResponse>(response);

                // Добавляем контракты с транзакциями > 0
                foreach (var contract in contractResponse.Items)
                {
                    if (contract.TxCount > 0)
                    {
                        contracts.Add(contract);
                    }
                    if (contracts.Count >= count) break;
                }

                // Проверяем наличие параметров для следующей страницы
                if (contractResponse.NextPageParams != null)
                {
                    // Формируем URL для следующего запроса
                    url = _urlBuild($"/smart-contracts?q={q}&filter=solidity&smart_contract_id={contractResponse.NextPageParams.SmartContractId}&coin_balance={contractResponse.NextPageParams.CoinBalance}");
                }
                else
                {
                    break; // Если нет параметров для следующей страницы, выходим из цикла
                }
            }

            return contracts;
        }

        public async Task<string> GetContractCode(string contractHash)
        {
            string contractUrl = _urlBuild($"smart-contracts/{contractHash}");

            // Выполняем запрос к API для получения кода контракта
            var response = await _httpClient.GetStringAsync(contractUrl);

            // Десериализуем JSON-ответ
            var contractDetails = JsonSerializer.Deserialize<ContractDetailsResponse>(response);

            return contractDetails.SourceCode ?? "Contract code not found.";
        }

        private string _urlBuild(string p) => string.Concat(_baseUrl, p);
    }
}
