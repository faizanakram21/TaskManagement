using Microsoft.Playwright;

namespace TaskManagement.AutomationTests.UI.Pages;

public class DashboardPage
{
    private readonly IPage _page;

    public DashboardPage(IPage page) => _page = page;

    public async Task<int> GetTotalTasksAsync() =>
        int.Parse(await _page.Locator(".stat-card.total h3").InnerTextAsync());

    public async Task<int> GetCompletedTasksAsync() =>
        int.Parse(await _page.Locator(".stat-card.completed h3").InnerTextAsync());

    public async Task<int> GetPendingTasksAsync() =>
        int.Parse(await _page.Locator(".stat-card.pending h3").InnerTextAsync());

    public Task ClickViewAllTasksAsync() =>
        _page.ClickAsync("a[routerLink='/tasks']");

    public Task ClickNewTaskAsync() =>
        _page.ClickAsync("a[routerLink='/tasks/add']");

    public Task<bool> IsLoadedAsync() =>
        _page.Locator(".dashboard-container").IsVisibleAsync();
}