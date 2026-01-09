using ParserPlugin.AzureDevOps.Automation.Domain.Models;

namespace ParserPlugin.AzureDevOps.Automation.Application.Parsing
{
    public class ParsedWorkItemInput
    {
        public string Organization { get; set; } = "";
        public string Project { get; set; } = "";
        public string AreaPath { get; set; } = "";
        public string Sprint { get; set; } = "";

        // Raízes da árvore (Epics, basicamente)
        public List<WorkItem> TasksTree { get; set; } = new();

        public void Print()
        {
            Console.WriteLine("Organization: " + Organization);
            Console.WriteLine("Project: " + Project);
            Console.WriteLine("AreaPath: " + AreaPath);
            Console.WriteLine("Sprint: " + Sprint);
            Console.WriteLine("Work Item Tree:");

            foreach (var root in TasksTree)
            {
                WorkItem.PrintWorkItem(root, 1);
            }
        }
    }
}
