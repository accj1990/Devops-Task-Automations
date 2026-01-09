using ParserPlugin.AzureDevOps.Automation.Application.Interfaces;
using ParserPlugin.AzureDevOps.Automation.Domain.Models;

namespace ParserPlugin.AzureDevOps.Automation.Application.Services
{
    public class WorkItemService
    {
        private readonly IAzureDevOpsClient _client;
        private readonly IDescriptionEnhancer _ai;

        public WorkItemService(IAzureDevOpsClient client, IDescriptionEnhancer ai)
        {
            _client = client;
            _ai = ai;
        }

        // 1) Cria todos os itens primeiro e preenche o Id
        public async Task CreateHierarchyAsync(WorkItem root)
        {
            await CreateAllNodesAsync(root, null);
        }

        // 2) Atualiza as relações entre os itens
        public async Task UpdateRelationsAsync(WorkItem root)
        {
            await UpdateRelationsRecursiveAsync(root);
        }

        private async Task CreateAllNodesAsync(WorkItem node, int? parentId)
        {
            if (!string.IsNullOrWhiteSpace(node.Description))
                node.Description = await _ai.ImproveAsync(node.Description);

            if (parentId.HasValue)
                node.ParentId = ( int )parentId;

            // cria o item e salva o Id no próprio nó
            node.Id = await _client.CreateWorkItemAsync(node);
            Console.WriteLine($"Created work ID: '{node.Id}' with Title: {node.Title}");
            if (node.Children != null)
            {
                foreach (var child in node.Children)
                {
                    try
                    {
                        await CreateAllNodesAsync(child, node.Id);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Error creating child work item '{child.Title}': {e.Message}");
                    }
                }
            }
        }

        private async Task UpdateRelationsRecursiveAsync(WorkItem node)
        {
            if (node.Id == 0)
                return;

            // Parent/Child/Related/Predecessor/Successor
            await _client.UpdateRelationsAsync(node.Id, node);

            if (node.Children != null)
            {
                foreach (var child in node.Children)
                {
                    await UpdateRelationsRecursiveAsync(child);
                }
            }
        }
    }
}
