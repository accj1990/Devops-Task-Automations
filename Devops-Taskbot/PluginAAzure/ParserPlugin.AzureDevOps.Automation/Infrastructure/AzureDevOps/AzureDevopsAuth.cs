using System.Net.Http.Headers;

public class AzureDevOpsAuth
{
    public static HttpClient CreateClient(string organization, string pat)
    {
        var client = new HttpClient
        {
            BaseAddress = new Uri($"https://dev.azure.com/{organization}/")
        };

        /*var token = Convert.ToBase64String(
            Encoding.ASCII.GetBytes($":{pat}")
        );*/
        var token = pat;
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        return client;
    }
}
