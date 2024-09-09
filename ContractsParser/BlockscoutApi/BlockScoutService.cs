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
            var uniqueContracts = new List<ContractItem>();

            while (uniqueContracts.Count < count)
            {
                // Выполняем запрос к API
                var response = await _httpClient.GetStringAsync(url);
                var contractResponse = JsonSerializer.Deserialize<ContractResponse>(response);

                // Собираем контракты с транзакциями > 0
                var allContracts = contractResponse.Items.Where(contract => contract.TxCount > 0).ToList();

                Console.WriteLine($"{allContracts.Count} contracts found");

                // Группируем контракты по имени в текущей партии, выбираем самый свежий
                var newUniqueContracts = allContracts
                    .GroupBy(contract => contract.Address.Name)
                    .Select(group => group.OrderByDescending(contract => contract.VerifiedAt).First())
                    .ToList();

                // Объединяем новые уникальные контракты с уже собранными и фильтруем окончательно
                uniqueContracts = uniqueContracts
                    .Concat(newUniqueContracts)
                    .GroupBy(contract => contract.Address.Name)
                    .Select(group => group.OrderByDescending(contract => contract.VerifiedAt).First())
                    .Take(count)
                    .ToList();

                Console.WriteLine($"{uniqueContracts.Count} unique in list");

                // Проверяем наличие параметров для следующей страницы
                if (contractResponse.NextPageParams != null && uniqueContracts.Count < count)
                {
                    // Формируем URL для следующего запроса
                    url = _urlBuild($"/smart-contracts?q={q}&filter=solidity&smart_contract_id={contractResponse.NextPageParams.SmartContractId}&coin_balance={contractResponse.NextPageParams.CoinBalance}");
                }
                else
                {
                    break; // Если нет параметров для следующей страницы или достигнуто необходимое количество, выходим из цикла
                }
            }

            foreach ( var item in uniqueContracts )
            {
                Console.WriteLine(item.Address.Name);
            }
            

            return uniqueContracts;
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
