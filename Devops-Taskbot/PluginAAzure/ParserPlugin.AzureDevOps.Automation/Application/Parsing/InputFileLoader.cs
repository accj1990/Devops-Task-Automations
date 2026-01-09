namespace ParserPlugin.AzureDevOps.Automation.Application.Parsing
{
    public class InputFileLoader
    {
        public async Task<string> LoadAsync(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException(
                    "Arquivo de input não encontrado", filePath);

            return await File.ReadAllTextAsync(filePath);
        }
    }
}
