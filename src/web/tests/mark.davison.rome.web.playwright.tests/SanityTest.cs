namespace mark.davison.rome.web.playwright.tests;

public sealed class SanityTest : RomeBaseTest
{
    [Test]
    public async Task Sanity()
    {
        await Task.Delay(TimeSpan.FromMilliseconds(10));
    }
}