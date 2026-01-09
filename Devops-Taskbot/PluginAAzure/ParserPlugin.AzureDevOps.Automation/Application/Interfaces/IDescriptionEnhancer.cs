namespace ParserPlugin.AzureDevOps.Automation.Application.Interfaces
{
    public interface IDescriptionEnhancer
    {
        Task<string> ImproveAsync(string rawText);
    }

}
