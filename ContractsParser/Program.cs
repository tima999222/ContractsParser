﻿using ContractsParser.BlockscoutApi;
using ContractsParser.ContractParser;
using ContractsParser.PromptGenerator;
using System.Diagnostics.Contracts;

class Program
{
    static async Task Main(string[] args)
    {
        IPromptGenerator promptGenerator = new FormulaPromptGenerator();
        ContractParser contractsParser = new ContractParser();
        HttpClient client = new HttpClient();
        IBlockScoutService blockScoutService = new BlockScoutService(client);

        var contracts = await blockScoutService.GetContracts(100, "Uniswap");

        Console.WriteLine($"Found {contracts.Count} contracts");

        var tasks = contracts.Select(async contract =>
        {
/*            try
            {*/
                string contractCode = await blockScoutService.GetContractCode(contract.Address.Hash);
                var funcs = contractsParser.ExtractFunctions(contractCode);

                Console.WriteLine($"found {funcs.Count} functions in {contract.Address.Hash}");

                var dict = _generateDict(contract.Address.Hash, funcs);
                await promptGenerator.GeneratePromptsInFilesAsync(dict);

                Console.WriteLine($"all prompts in {contract.Address.Hash} were generated");
            /*}*/
/*            catch (Exception ex)
            {
                Console.WriteLine($"Error processing contract {contract.Address.Hash}: {ex.Message}");
            }*/
        });

        await Task.WhenAll(tasks);
    }

    private static Dictionary<string, string> _generateDict(string hash, List<string> funcs)
    {
        var dict = new Dictionary<string, string>();
        int count = 1;

        foreach (var func in funcs)
        {
            dict.Add($"C:\\Users\\Тимофей\\source\\repos\\ContractsParser\\ContractsParser\\output\\prompt-{hash}-{count}.txt", func);
            count++;
        }
        return dict;
    }
}

