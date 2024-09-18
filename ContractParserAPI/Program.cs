using System.Text.RegularExpressions;
using ContractsParser.BlockscoutApi;
using ContractsParser.ContractParser;
using ContractsParser.PromptGenerator;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.MapGet("/getPrompts/{count}", (int count) =>
    {
        return GetPromptsAsync(count);
    })
    .WithName("GetPrompts")
    .WithOpenApi();

app.MapGet("/getPromptsWithQuery/{count}/{query}", (int count, string query = "") =>
    {
        return GetPromptsAsync(count, query);
    })
    .WithName("GetPromptsWithQuery")
    .WithOpenApi();

app.Run();

static Dictionary<string, string> _generateDict(string name, List<string> funcs)
{
    var dict = new Dictionary<string, string>();

    foreach (var func in funcs)
    {
        var funcName = ExtractFunctionName(func);

        if (funcName != null)
        {
            dict.Add($"prompt-{name}-{funcName}", func);
        }
    }
    return dict;
}

static string? ExtractFunctionName(string input)
{
    string pattern = @"function\s+(\w+)\s*\(";
    Match match = Regex.Match(input, pattern);

    if (match.Success)
    {
        return match.Groups[1].Value;
    }

    return null;
}

static async Task<List<KeyValuePair<string, string>>> GetPromptsAsync(int count, string query = "")
{
    IPromptGenerator promptGenerator = new FormulaPromptGenerator();
    ContractParser contractsParser = new ContractParser();
    HttpClient client = new HttpClient();
    IBlockScoutService blockScoutService = new BlockScoutService(client);

    var contracts = await blockScoutService.GetContracts(count, query);

    Console.WriteLine($"Found {contracts.Count} contracts");

    var prompts = new List<KeyValuePair<string, string>>();
        
    var tasks = contracts.Select(async contract =>
    {
        try
        {
            string contractCode = await blockScoutService.GetContractCode(contract.Address.Hash);
            var funcs = contractsParser.ExtractFunctions(contractCode);

            Console.WriteLine($"found {funcs.Count} functions in {contract.Address.Hash}");

            var dict = _generateDict(contract.Address.Name, funcs);
            var tempPrompts = await promptGenerator.GeneratePromptsAsync(dict);
            prompts.AddRange(tempPrompts);

            Console.WriteLine($"all prompts in {contract.Address.Name} were generated");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing contract {contract.Address.Name}: {ex.Message}");
        }
    });

    await Task.WhenAll(tasks);

    return prompts;
}