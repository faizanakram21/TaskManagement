using Microsoft.Extensions.Configuration;

namespace TaskManagement.AutomationTests.Config;

public class EndpointSettings
{
    public string Register { get; set; } = "/api/auth/register";
    public string Login { get; set; } = "/api/auth/login";
    public string RefreshToken { get; set; } = "/api/auth/refresh";
    public string Tasks { get; set; } = "/api/tasks";
}

public class TestUserSettings
{
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
    public string Name { get; set; } = "";
}

public class PlaywrightSettings
{
    public bool Headless { get; set; } = true;
    public int SlowMoMs { get; set; } = 0;
    public int DefaultTimeoutMs { get; set; } = 15000;
}

public class TestSettings
{
    public string ApiBaseUrl { get; set; } = "";
    public string UiBaseUrl { get; set; } = "";
    public string DbConnectionString { get; set; } = "";
    public EndpointSettings Endpoints { get; set; } = new();
    public TestUserSettings TestUser { get; set; } = new();
    public PlaywrightSettings Playwright { get; set; } = new();

    private static TestSettings? _instance;

    public static TestSettings Instance
    {
        get
        {
            if (_instance is not null) return _instance;

            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.test.json", optional: false, reloadOnChange: false)
                .AddEnvironmentVariables()
                .Build();

            _instance = new TestSettings();
            config.Bind(_instance);
            return _instance;
        }
    }
}