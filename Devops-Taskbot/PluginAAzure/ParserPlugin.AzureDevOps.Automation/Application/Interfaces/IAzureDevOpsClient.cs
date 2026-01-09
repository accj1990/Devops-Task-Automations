using ParserPlugin.AzureDevOps.Automation.Domain.Models;

namespace ParserPlugin.AzureDevOps.Automation.Application.Interfaces
{
    public interface IAzureDevOpsClient
    {
        Task<int> CreateWorkItemAsync(WorkItem task);
        Task UpdateRelationsAsync(int workItemId, WorkItem node);
    }
}
