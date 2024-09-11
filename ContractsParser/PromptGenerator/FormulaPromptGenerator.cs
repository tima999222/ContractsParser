namespace ContractsParser.PromptGenerator
{
    public class FormulaPromptGenerator : IPromptGenerator
    {
        private string _basePrompt = "\"\"\"Make a C# Formula method for this Solidity contract method:\r\n\r\n{0}\r\n\r\nReturn only a C# code. Follow all the code and answer rules in your system prompt\"\"\"";

        public async Task<List<KeyValuePair<string, string>>> GeneratePromptsAsync(Dictionary<string, string> fileOutputParamMap)
        {
            var tasks = new List<Task>();
            var data = new List<KeyValuePair<string, string>>();

            foreach (var entry in fileOutputParamMap)
            {
                string param = entry.Value;
                string name = entry.Key;

                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        string prompt = string.Format(_basePrompt, param);
                        data.Add(new KeyValuePair<string, string>(name, prompt));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }));
            }

            await Task.WhenAll(tasks);

            return data;
        }
    }
}
