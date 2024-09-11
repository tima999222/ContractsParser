namespace ContractsParser.PromptGenerator
{
    public interface IPromptGenerator
    {
        public Task<List<KeyValuePair<string, string>>> GeneratePromptsAsync(Dictionary<string, string> fileOutputParamMap);
    }
}
