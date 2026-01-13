namespace mark.davison.rome.web.playwright.app;

public sealed class DummyTest
{
    [Test]
    public async Task ToMakeThingsWork()
    {
        await Assert.That(() => { }).ThrowsNothing();
    }
}
