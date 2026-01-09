using ParserPlugin.AzureDevOps.Automation.Domain.Models;

namespace ParserPlugin.AzureDevOps.Automation.Application.Parsing
{
    public class WorkItemFileParser
    {
        public ParsedWorkItemInput Parse(string text)
        {
            var input = new ParsedWorkItemInput();

            // agora a raiz da árvore
            input.TasksTree = new List<WorkItem>();

            WorkItem? currentEpic = null;
            WorkItem? currentFeature = null;
            WorkItem? currentStory = null;

            WorkItem? lastCreated = null;

            string currentAssignee = "";
            string currentArea = "";

            var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                            .Select(l => l.Trim())
                            .ToList();

            foreach (var raw in lines)
            {
                var line = raw.Trim();
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                // ---------- HEADER ----------
                if (line.StartsWith("ORG:", StringComparison.OrdinalIgnoreCase))
                {
                    input.Organization = line.Replace("ORG:", "").Trim();
                    continue;
                }

                if (line.StartsWith("PROJECT:", StringComparison.OrdinalIgnoreCase))
                {
                    input.Project = line.Replace("PROJECT:", "").Trim();
                    continue;
                }

                if (line.StartsWith("AREA:", StringComparison.OrdinalIgnoreCase))
                {
                    input.AreaPath = line.Replace("AREA:", "").Trim();
                    continue;
                }

                if (line.StartsWith("SPRINT:", StringComparison.OrdinalIgnoreCase))
                {
                    input.Sprint = line.Replace("SPRINT:", "").Trim();
                    continue;
                }

                // ---------- CONTEXTO DE BLOCO ----------
                if (line.StartsWith("@"))
                {
                    var parts = line.Split('|');

                    currentArea = parts[0].Trim().TrimStart('@');
                    currentAssignee = parts.Length > 1 ? parts[1].Trim() : "";

                    continue;
                }

                // ---------- EPIC ----------
                if (line.Contains("EPIC:", StringComparison.OrdinalIgnoreCase))
                {
                    currentEpic = Create("Epic", ExtractTitle(line), currentAssignee, currentArea);

                    // raiz da árvore
                    input.TasksTree.Add(currentEpic);

                    currentFeature = null;
                    currentStory = null;

                    lastCreated = currentEpic;
                    continue;
                }

                // ---------- FEATURE ----------
                if (line.Contains("FEATURE:", StringComparison.OrdinalIgnoreCase))
                {
                    currentFeature = Create("Feature", ExtractTitle(line), currentAssignee, currentArea);

                    // filho da EPIC
                    currentEpic?.Children.Add(currentFeature);

                    currentStory = null;

                    lastCreated = currentFeature;
                    continue;
                }

                // ---------- USER STORY ----------
                if (line.Contains("USER STORY:", StringComparison.OrdinalIgnoreCase))
                {
                    currentStory = Create("User Story", ExtractTitle(line), currentAssignee, currentArea);

                    // filho da Feature
                    currentFeature?.Children.Add(currentStory);

                    lastCreated = currentStory;
                    continue;
                }

                // ---------- TASK ----------
                if (line.StartsWith("TASK:", StringComparison.OrdinalIgnoreCase))
                {
                    var task = Create("Task", ExtractTitle(line), currentAssignee, currentArea);

                    // sempre filho da story
                    currentStory?.Children.Add(task);

                    lastCreated = task;
                    continue;
                }

                // ---------- TEST CASE ----------
                if (line.StartsWith("TEST CASE:", StringComparison.OrdinalIgnoreCase))
                {
                    var test = Create("Test Case", ExtractTitle(line), currentAssignee, currentArea);

                    // também filho da story
                    currentStory?.Children.Add(test);

                    lastCreated = test;
                    continue;
                }

                // ---------- DESCRIPTION ----------
                if (line.StartsWith("DESC:", StringComparison.OrdinalIgnoreCase))
                {
                    if (lastCreated != null)
                        lastCreated.Description = line.Replace("DESC:", "").Trim();

                    continue;
                }

                // ---------- TAGS ----------
                if (line.StartsWith("TAGS:", StringComparison.OrdinalIgnoreCase))
                {
                    if (lastCreated != null)
                    {
                        var tagsText = line.Replace("TAGS:", "").Trim();

                        lastCreated.Tags = tagsText
                            .Split(';', StringSplitOptions.RemoveEmptyEntries)
                            .Select(t => t.Trim())
                            .ToList();
                    }

                    continue;
                }

                // ---------- STORY POINTS ----------
                if (line.StartsWith("STORY POINTS", StringComparison.OrdinalIgnoreCase))
                {
                    if (lastCreated != null)
                    {
                        var rawValue = line.Split(':', 2)[1].Trim();
                        lastCreated.StoryPoints = rawValue;
                    }

                    continue;
                }

                // ---------- PRIORITY ----------
                if (line.StartsWith("PRIORITY", StringComparison.OrdinalIgnoreCase))
                {
                    if (lastCreated != null)
                        lastCreated.Priority = line.Split(':', 2)[1].Trim();

                    continue;
                }

                // ---------- RISK ----------
                if (line.StartsWith("RISK", StringComparison.OrdinalIgnoreCase))
                {
                    if (lastCreated != null)
                        lastCreated.Risk = line.Split(':', 2)[1].Trim();

                    continue;
                }

                // ---------- ACCEPTANCE CRITERIA ----------
                if (line.StartsWith("ACCEPTANCE CRITERIA", StringComparison.OrdinalIgnoreCase))
                {
                    if (lastCreated != null)
                        lastCreated.AcceptanceCriteria = line.Split(':', 2)[1].Trim();

                    continue;
                }
            }

            return input;
        }

        private static WorkItem Create(string type, string title, string assignee, string area)
        {
            return new WorkItem
            {
                Type = type,
                Title = title,
                AssignedTo = assignee,
                Area = area,
                Children = new List<WorkItem>()
            };
        }

        private static string ExtractTitle(string line)
        {
            line = line.Replace("[PRINCIPAL]", "").Trim();

            var parts = line.Split(':', 2);

            return parts.Length == 2
                ? parts[1].Trim()
                : line.Trim();
        }
    }
}
