namespace mark.davison.common.web.playwright.test;

public sealed class DummyTest
{
    [Test]
    public async Task ToMakeThingsWork()
    {
        await Assert.That(() => { }).ThrowsNothing();
    }
}
