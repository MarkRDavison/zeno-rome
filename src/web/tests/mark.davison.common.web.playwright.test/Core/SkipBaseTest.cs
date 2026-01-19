namespace mark.davison.common.web.playwright.test.Core;

public class SkipBaseTest : IAsyncDisposable
{
    protected readonly Faker _faker = new Faker();

    protected SkipBaseTest(IDataNames _)
    {

    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }

}
