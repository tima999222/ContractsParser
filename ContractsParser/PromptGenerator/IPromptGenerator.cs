namespace ContractsParser.PromptGenerator
{
    public interface IPromptGenerator
    {
        public Task GeneratePromptsInFilesAsync(Dictionary<string, string> fileOutputParamMap);
    }
}
