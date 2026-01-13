namespace mark.davison.common.web.playwright.test.Core;

public class SkipBaseTest : IAsyncDisposable
{
    protected SkipBaseTest(IDataNames _)
    {

    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}
