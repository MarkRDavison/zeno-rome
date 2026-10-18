namespace mark.davison.rome.web.playwright.tests.Scenarios.Dashboard;

public sealed class DashboardTests : RomeBaseTest
{
    [Test]
    public async Task Home_ShowsAccountsSection()
    {
        await CurrentPage.GetByTestId(DataTestIds.DashboardAccounts).WaitForAsync();
    }
}
