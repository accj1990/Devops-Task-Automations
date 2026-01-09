using ParserPlugin.AzureDevOps.Automation.Application.Interfaces;

namespace ParserPlugin.AzureDevOps.Automation.Infrastructure.AI
{
    public class MockDescriptionEnhancer : IDescriptionEnhancer
    {
        public Task<string> ImproveAsync(string rawText)
        {
            return Task.FromResult($"{rawText}\n\nAcceptance Criteria:\n- Funciona conforme esperado\n- Testado\n- Documentado");
        }
    }
}
