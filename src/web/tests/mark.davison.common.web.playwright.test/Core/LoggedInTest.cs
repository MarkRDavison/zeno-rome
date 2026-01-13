namespace mark.davison.common.web.playwright.test.Core;

public abstract class LoggedInTest : BaseTest
{
    protected LoggedInTest(IDataNames dataNames) : base(dataNames)
    {
    }

    protected override async Task OnTestInitialise()
    {
        await AuthenticationHelper.EnsureLoggedIn(CurrentPage);
    }
}
