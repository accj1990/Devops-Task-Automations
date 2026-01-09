namespace ParserPlugin.AzureDevOps.Automation.Domain.Models
{
    public class WorkItem
    {
        // --- nó da árvore ---
        public int Id { get; set; } = 0; // Id do Azure DevOps após criação 
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string AssignedTo { get; set; } = "";
        public string Type { get; set; } = ""; // Epic, Feature, User Story, Task, Test Case, etc.
        public string Area { get; set; } = "";

        public List<string> Tags { get; set; } = new();  // tags sem dor de cabeça
        public string AcceptanceCriteria { get; set; } = "";
        public string StoryPoints { get; set; } = "";
        public string Priority { get; set; } = "";
        public string Risk { get; set; } = "";

        // mantém referência lógica, útil para persistir no Azure
        public int ParentId { get; set; } = 0;
        public List<int> PredecessorIds { get; set; } = [];
        public List<int> SuccessorIds { get; set; } = [];
        public List<int> RelatedIds { get; set; } = [];

        // filhos diretos na árvore
        public List<WorkItem> Children { get; set; } = new();

        public static void PrintWorkItem(WorkItem item, int indent = 0)
        {
            string pad = new string(' ', indent * 2);

            Console.WriteLine($"{pad}Title: {item.Title}");
            Console.WriteLine($"{pad}Type: {item.Type}");
            Console.WriteLine($"{pad}Description: {item.Description}");
            Console.WriteLine($"{pad}Assigned To: {item.AssignedTo}");
            Console.WriteLine($"{pad}Area: {item.Area}");
            Console.WriteLine($"{pad}Tags: {string.Join("; ", item.Tags)}");
            Console.WriteLine($"{pad}Story Points: {item.StoryPoints}");
            Console.WriteLine($"{pad}Priority: {item.Priority}");
            Console.WriteLine($"{pad}Risk: {item.Risk}");
            Console.WriteLine($"{pad}Acceptance Criteria: {item.AcceptanceCriteria}");

            Console.WriteLine($"{pad}--------------------------");

            foreach (var child in item.Children)
            {
                // recursão linda e sem medo
                PrintWorkItem(child, indent + 1);
            }
        }
    }
}
