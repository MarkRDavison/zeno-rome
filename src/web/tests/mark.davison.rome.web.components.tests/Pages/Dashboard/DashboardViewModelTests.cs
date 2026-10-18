using mark.davison.rome.web.components.Pages.Dashboard;

namespace mark.davison.rome.web.components.tests.Pages.Dashboard;

public sealed class DashboardViewModelTests
{
    [Test]
    public async Task OnAppContextUpdated_RefetchesHomeWhenInitialized()
    {
        var appContext = new Mock<IAppContextService>();
        var month = DateOnly.FromDateTime(DateTime.Today).GetMonthRange();
        appContext.Setup(_ => _.State).Returns(new AppContextState(month.Item1, month.Item2));

        var homeState = new Mock<IHomeState>();
        homeState.Setup(_ => _.Loading).Returns(false);
        homeState.Setup(_ => _.Home).Returns(new HomeQueryValue());
        homeState.Setup(_ => _.FetchState()).Returns(Task.CompletedTask);

        var startupState = new Mock<IStartupState>();
        startupState.Setup(_ => _.BootstrapComplete).Returns(true);
        startupState.Setup(_ => _.Currencies).Returns([]);
        startupState.Setup(_ => _.AccountTypes).Returns([]);

        var bootstrap = new Mock<IAppSessionBootstrap>();
        bootstrap.Setup(_ => _.EnsureAuthenticatedStartupAsync()).Returns(Task.CompletedTask);

        var viewModel = new DashboardViewModel(
            startupState.Object,
            homeState.Object,
            appContext.Object,
            bootstrap.Object);

        await viewModel.Initialize(null);

        appContext.Raise(_ => _.RangeUpdated += null, EventArgs.Empty);

        await Task.Delay(50);

        homeState.Verify(_ => _.FetchState(), Times.AtLeast(2));
    }
}
