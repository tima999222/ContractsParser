namespace ContractsParser.PromptGenerator
{
    public interface IPromptGenerator
    {
        public Task<List<string>> GeneratePromptsAsync(Dictionary<string, string> fileOutputParamMap);
    }
}
