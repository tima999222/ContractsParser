using ContractsParser.DTOs;

namespace ContractsParser.BlockscoutApi
{
    public interface IBlockScoutService
    {
        public Task<List<ContractItem>> GetContracts(int count, string q = "");
        public Task<string> GetContractCode(string contractHash);
    }
}
