using RestSharp;
using TaskManagement.AutomationTests.Config;

namespace TaskManagement.AutomationTests.API.Clients;

public class AuthApiClient : BaseApiClient
{
    public RestResponse<AuthResponseDto> Register(RegisterDto dto)
    {
        var request = new RestRequest(TestSettings.Instance.Endpoints.Register, Method.Post)
            .AddJsonBody(dto);
        return Client.Execute<AuthResponseDto>(request);
    }

    public RestResponse<AuthResponseDto> Login(LoginDto dto)
    {
        var request = new RestRequest(TestSettings.Instance.Endpoints.Login, Method.Post)
            .AddJsonBody(dto);
        return Client.Execute<AuthResponseDto>(request);
    }

    public RestResponse Login_Raw(LoginDto dto)
    {
        var request = new RestRequest(TestSettings.Instance.Endpoints.Login, Method.Post)
            .AddJsonBody(dto);
        return Client.Execute(request);
    }

    public string GetValidToken()
    {
        var settings = TestSettings.Instance.TestUser;

        var loginResponse = Login(new LoginDto(settings.Email, settings.Password));
        if (loginResponse.IsSuccessful && loginResponse.Data is not null)
        {
            return loginResponse.Data.Token;
        }

        Register(new RegisterDto(settings.Name, settings.Email, settings.Password));
        loginResponse = Login(new LoginDto(settings.Email, settings.Password));

        if (!loginResponse.IsSuccessful || loginResponse.Data is null)
        {
            throw new InvalidOperationException(
                $"Could not obtain auth token. Status: {loginResponse.StatusCode}, Body: {loginResponse.Content}");
        }

        return loginResponse.Data.Token;
    }
}