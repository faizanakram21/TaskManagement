using FluentAssertions;
using TaskManagement.AutomationTests.API.Clients;
using TaskManagement.AutomationTests.Config;
using TaskManagement.AutomationTests.Helpers;
using TaskManagement.AutomationTests.UI.Pages;

namespace TaskManagement.AutomationTests.UI.Tests;

[TestFixture]
[Category("UI")]
public class LoginUiTests : PlaywrightFixture
{
    [Test]
    public async Task Login_WithValidCredentials_RedirectsToDashboard()
    {
        var (name, email, password) = TestDataBuilder.NewRegisterUser();
        new AuthApiClient().Register(new RegisterDto(name, email, password));

        var loginPage = new LoginPage(Page);
        await loginPage.GoToAsync();
        await loginPage.LoginAsync(email, password);

        await Page.WaitForURLAsync($"{TestSettings.Instance.UiBaseUrl}/dashboard");

        var dashboard = new DashboardPage(Page);
        (await dashboard.IsLoadedAsync()).Should().BeTrue();
    }

    [Test]
    public async Task Login_WithInvalidCredentials_ShowsErrorAndStaysOnLoginPage()
    {
        var loginPage = new LoginPage(Page);
        await loginPage.GoToAsync();
        await loginPage.LoginAsync($"ghost.{Guid.NewGuid():N}@example.com", "WrongPass@123");

        (await loginPage.HasErrorMessageAsync()).Should().BeTrue();
        Page.Url.Should().Contain("/login");
    }
}