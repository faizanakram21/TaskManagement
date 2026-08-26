using Microsoft.Playwright;
using TaskManagement.AutomationTests.Config;

namespace TaskManagement.AutomationTests.UI.Pages;

public class LoginPage
{
    private readonly IPage _page;

    public LoginPage(IPage page) => _page = page;

    public async Task GoToAsync()
    {
        await _page.GotoAsync($"{TestSettings.Instance.UiBaseUrl}/login");
    }

    public async Task LoginAsync(string email, string password)
    {
        await _page.FillAsync("input[formcontrolname='email']", email);
        await _page.FillAsync("input[formcontrolname='password']", password);
        await _page.ClickAsync("button[type='submit']");
    }

    public async Task<bool> HasErrorMessageAsync()
    {
        try
        {
            await _page.WaitForSelectorAsync(".error-box", new PageWaitForSelectorOptions { Timeout = 5000 });
            return true;
        }
        catch (TimeoutException)
        {
            return false;
        }
    }
}