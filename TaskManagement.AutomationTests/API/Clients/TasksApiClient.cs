using RestSharp;
using TaskManagement.AutomationTests.Config;

namespace TaskManagement.AutomationTests.API.Clients;

public class TasksApiClient : BaseApiClient
{
    public RestResponse<List<TaskDto>> GetAll(string token)
    {
        var request = new RestRequest(TestSettings.Instance.Endpoints.Tasks, Method.Get);
        AddAuthHeader(request, token);
        return Client.Execute<List<TaskDto>>(request);
    }

    public RestResponse<TaskDto> Create(TaskDto dto, string token)
    {
        var request = new RestRequest(TestSettings.Instance.Endpoints.Tasks, Method.Post)
            .AddJsonBody(dto);
        AddAuthHeader(request, token);
        return Client.Execute<TaskDto>(request);
    }

    public RestResponse<TaskDto> Update(int id, TaskDto dto, string token)
    {
        var request = new RestRequest($"{TestSettings.Instance.Endpoints.Tasks}/{id}", Method.Put)
            .AddJsonBody(dto);
        AddAuthHeader(request, token);
        return Client.Execute<TaskDto>(request);
    }

    public RestResponse Delete(int id, string token)
    {
        var request = new RestRequest($"{TestSettings.Instance.Endpoints.Tasks}/{id}", Method.Delete);
        AddAuthHeader(request, token);
        return Client.Execute(request);
    }
}