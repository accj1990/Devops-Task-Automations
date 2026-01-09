using ParserPlugin.AzureDevOps.Automation.Application.Interfaces;
using ParserPlugin.AzureDevOps.Automation.Application.Parsing;
using ParserPlugin.AzureDevOps.Automation.Application.Services;
using ParserPlugin.AzureDevOps.Automation.Infrastructure.AI;

namespace GenericConsoleAppAzureTeste
{

    public class Program
    {
        public static async Task Main(string[] args)
        {
            var basePath = AppContext.BaseDirectory;

            var inputFilePath = Path.Combine(
                basePath,
                "Inputs",
                "story.txt"
            );

            if (!File.Exists(inputFilePath))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Arquivo não encontrado: {inputFilePath}");
                Console.ResetColor();
                return;
            }

            Console.WriteLine("Ler arquivo de entrada...");
            var fileText = await File.ReadAllTextAsync(inputFilePath);

            Console.WriteLine(" Parser do conteúdo...");
            var parser = new WorkItemFileParser();
            ParsedWorkItemInput input = parser.Parse(fileText);

            //input.Print(); // Opcional: Imprime o input parseado para verificação

            Console.WriteLine("Conectando no Azure DevOps...");
            var http = AzureDevOpsAuth.CreateClient(
                input.Organization,
                "TOKEN"
            );

            IAzureDevOpsClient client =
                new AzureDevOpsClient(http, input.Project);

            IDescriptionEnhancer ai =
                new MockDescriptionEnhancer();

            var service =
                new WorkItemService(client, ai);

            Console.WriteLine("Criando itens no Azure DevOps...");

            foreach (var workItem in input.TasksTree)
            {
                // Cria a hierarquia completa de work items
                await service.CreateHierarchyAsync(workItem);

                // Atualiza as relações entre os work items
                await service.UpdateRelationsAsync(workItem);
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Work items criados com sucesso.");
            Console.ResetColor();
        }
    }
}