namespace mark.davison.rome.web.components.tests.Layouts;

public sealed class NavMenuTagsTests
{
    [Test]
    public async Task ClickingNavTags_OpensTagManagerShell()
    {
        var shell = new Mock<ITagManagerShell>();

        using var ctx = new RomeComponentTestContext();
        ctx.Services.AddSingleton(shell.Object);

        var cut = ctx.RenderComponent<NavMenu>();
        cut.Find($"[data-testid='{DataTestIds.NavTags}']").Click();

        shell.Verify(_ => _.Open(), Times.Once);
    }
}
