namespace mark.davison.rome.web.playwright.tests;

public sealed class DummyTest
{
    [Test]
    public async Task ToMakeThingsWork()
    {
        await Assert.That(() => { }).ThrowsNothing();
    }
}
