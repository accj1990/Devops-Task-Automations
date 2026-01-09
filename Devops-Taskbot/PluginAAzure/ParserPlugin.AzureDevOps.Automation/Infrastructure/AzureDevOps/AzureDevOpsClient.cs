using System.Text;
using System.Text.Json;
using ParserPlugin.AzureDevOps.Automation.Application.Interfaces;
using ParserPlugin.AzureDevOps.Automation.Domain.Models;

public class AzureDevOpsClient : IAzureDevOpsClient
{
    private readonly HttpClient _http;
    private readonly string _project;

    public AzureDevOpsClient(HttpClient http, string project)
    {
        _http = http;
        _project = project;
    }

    public async Task<int> CreateWorkItemAsync(WorkItem workItem)
    {
        var patch = new List<object>
        {
            new { op = "add", path = "/fields/System.Title", value = workItem.Title },
            new { op = "add", path = "/fields/System.Description", value = workItem.Description },
            new { op = "add", path = "/fields/System.AssignedTo", value = workItem.AssignedTo }
        };

        if (!string.IsNullOrWhiteSpace(workItem.AcceptanceCriteria))
            patch.Add(new { op = "add", path = "/fields/Microsoft.VSTS.Common.AcceptanceCriteria", value = workItem.AcceptanceCriteria });

        if (!string.IsNullOrWhiteSpace(workItem.StoryPoints))
            patch.Add(new { op = "add", path = "/fields/StoryPoints", value = workItem.StoryPoints });

        if (!string.IsNullOrWhiteSpace(workItem.Priority))
            patch.Add(new { op = "add", path = "/fields/Microsoft.VSTS.Common.Priority", value = workItem.Priority });

        if (!string.IsNullOrWhiteSpace(workItem.Risk))
            patch.Add(new { op = "add", path = "/fields/Microsoft.VSTS.Common.Risk", value = workItem.Risk });

        if (workItem.Tags != null && workItem.Tags.Count > 0)
            patch.Add(new { op = "add", path = "/fields/System.Tags", value = string.Join(";", workItem.Tags) });

        var jsonPayload = JsonSerializer.Serialize(patch);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json-patch+json");
        var workItemType = Uri.EscapeDataString(workItem.Type);

        var url = _http.BaseAddress + $"{_project}/_apis/wit/workitems/${workItemType}?api-version=7.2-preview.3";
        Console.WriteLine($"Criando work item do tipo: '{url}'");
        Console.WriteLine($"JSON enviado: '{jsonPayload}'");
        var response = await _http.PostAsync($"{_project}/_apis/wit/workitems/${workItemType}?api-version=7.2-preview.3", content);

        if (!response.IsSuccessStatusCode)
        {
            var contentResponse = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Erro {response.StatusCode}: {contentResponse}");
            throw new Exception($"Falha ao criar ou atualizar Work Item {workItemType} e : contentResponse : {contentResponse}");
        }

        Console.WriteLine($"JSON enviado status: '{"OK"}'");

        var json = await response.Content.ReadAsStringAsync();
        return JsonDocument.Parse(json).RootElement.GetProperty("id").GetInt32();
    }

    public async Task UpdateRelationsAsync(int workItemId, WorkItem node)
    {
        var relationsPatch = new List<object>();
        var urlBase = _http.BaseAddress + $"{_project}/_apis/wit/workitems/";

        if (node.ParentId > 0)
            relationsPatch.Add(new
            {
                op = "add",
                path = "/relations/-",
                value = new
                {
                    rel = "System.LinkTypes.Hierarchy-Reverse",
                    url = urlBase + $"{node.ParentId}"
                }
            });

        if (node.Children != null)
        {
            foreach (var child in node.Children)
            {
                if (child.Id > 0)
                    relationsPatch.Add(new
                    {
                        op = "add",
                        path = "/relations/-",
                        value = new
                        {
                            rel = "System.LinkTypes.Hierarchy-Forward",
                            url = urlBase + $"{child.Id}"
                        }
                    });
            }
        }

        if (node.RelatedIds != null)
        {
            foreach (var relatedId in node.RelatedIds)
            {
                relationsPatch.Add(new
                {
                    op = "add",
                    path = "/relations/-",
                    value = new
                    {
                        rel = "System.LinkTypes.Related",
                        url = urlBase + $"{relatedId}"
                    }
                });
            }
        }

        if (node.PredecessorIds != null)
        {
            foreach (var predId in node.PredecessorIds)
            {
                relationsPatch.Add(new
                {
                    op = "add",
                    path = "/relations/-",
                    value = new
                    {
                        rel = "System.LinkTypes.Dependency-Forward",
                        url = urlBase + $"{predId}"
                    }
                });
            }
        }

        if (node.SuccessorIds != null)
        {
            foreach (var succId in node.SuccessorIds)
            {
                relationsPatch.Add(new
                {
                    op = "add",
                    path = "/relations/-",
                    value = new
                    {
                        rel = "System.LinkTypes.Dependency-Reverse",
                        url = urlBase + $"{succId}"
                    }
                });
            }
        }

        if (relationsPatch.Count == 0)
            return;

        Console.WriteLine($"Atualizando relações do work item ID: '{workItemId}'");
        var jsonPayload = JsonSerializer.Serialize(relationsPatch);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json-patch+json");
        var response = await _http.PatchAsync($"{_project}/_apis/wit/workitems/{workItemId}?api-version=7.2-preview.3", content);

        if (!response.IsSuccessStatusCode)
        {
            var contentResponse = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Erro {response.StatusCode}: {contentResponse}");
            throw new Exception($"Falha ao criar ou atualizar Work Item {workItemId} e : contentResponse : {contentResponse}");
        }
    }
}
