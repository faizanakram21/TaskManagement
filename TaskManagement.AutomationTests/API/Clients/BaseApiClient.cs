using RestSharp;
using TaskManagement.AutomationTests.Config;

namespace TaskManagement.AutomationTests.API.Clients;

public abstract class BaseApiClient
{
    protected readonly RestClient Client;

    protected BaseApiClient()
    {
        var options = new RestClientOptions(TestSettings.Instance.ApiBaseUrl)
        {
            RemoteCertificateValidationCallback = (_, _, _, _) => true
        };
        Client = new RestClient(options);
    }

    protected static void AddAuthHeader(RestRequest request, string? token)
    {
        if (!string.IsNullOrWhiteSpace(token))
        {
            request.AddHeader("Authorization", $"Bearer {token}");
        }
    }
}