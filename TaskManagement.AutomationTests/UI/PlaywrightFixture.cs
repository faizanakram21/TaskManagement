using Microsoft.Playwright;
using NUnit.Framework;
using TaskManagement.AutomationTests.Config;

namespace TaskManagement.AutomationTests.UI;

[TestFixture]
public abstract class PlaywrightFixture
{
    protected IPlaywright PlaywrightInstance = null!;
    protected IBrowser Browser = null!;
    protected IPage Page = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        PlaywrightInstance = await Playwright.CreateAsync();
        Browser = await PlaywrightInstance.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = TestSettings.Instance.Playwright.Headless,
            SlowMo = TestSettings.Instance.Playwright.SlowMoMs
        });
    }

    [SetUp]
    public async Task SetUp()
    {
        var context = await Browser.NewContextAsync();
        context.SetDefaultTimeout(TestSettings.Instance.Playwright.DefaultTimeoutMs);
        Page = await context.NewPageAsync();
    }

    [TearDown]
    public async Task TearDown()
    {
        await Page.Context.CloseAsync();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await Browser.DisposeAsync();
        PlaywrightInstance.Dispose();
    }
}