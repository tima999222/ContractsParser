using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ContractsParser.PromptGenerator
{
    public class FormulaPromptGenerator : IPromptGenerator
    {
        private string _basePrompt = "\"\"\"Make a C# Formula method for this Solidity contract method:\r\n\r\n{0}\r\n\r\nReturn only a C# code. Follow all the code and answer rules in your system prompt\"\"\"";

        public async Task GeneratePromptsInFilesAsync(Dictionary<string, string> fileOutputParamMap)
        {
            var tasks = new List<Task>();

            foreach (var entry in fileOutputParamMap)
            {
                string fileOutput = entry.Key;
                string param = entry.Value;

                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        string prompt = string.Format(_basePrompt, param);
                        await File.WriteAllTextAsync(fileOutput, prompt);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }));
            }

            await Task.WhenAll(tasks);
        }
    }
}
