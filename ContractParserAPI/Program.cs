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
    int count = 1;

    foreach (var func in funcs)
    {
        dict.Add($"ContractsParser\\output\\prompt-{name}-{count}.txt", func);
        count++;
    }
    return dict;
}

static async Task<List<string>> GetPromptsAsync(int count, string query = "")
{
    IPromptGenerator promptGenerator = new FormulaPromptGenerator();
    ContractParser contractsParser = new ContractParser();
    HttpClient client = new HttpClient();
    IBlockScoutService blockScoutService = new BlockScoutService(client);

    var contracts = await blockScoutService.GetContracts(count, query);

    Console.WriteLine($"Found {contracts.Count} contracts");

    var prompts = new List<string>();
        
    var tasks = contracts.Select(async contract =>
    {
        try
        {
            string contractCode = await blockScoutService.GetContractCode(contract.Address.Hash);
            var funcs = contractsParser.ExtractFunctions(contractCode);

            Console.WriteLine($"found {funcs.Count} functions in {contract.Address.Hash}");

            var dict = _generateDict(contract.Address.Name, funcs);
            prompts = await promptGenerator.GeneratePromptsAsync(dict);

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