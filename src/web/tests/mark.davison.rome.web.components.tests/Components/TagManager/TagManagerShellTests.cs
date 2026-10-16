namespace mark.davison.rome.web.components.tests.Components.TagManager;

public sealed class TagManagerShellTests
{
    [Test]
    public async Task OpenClose_RaisesOpenChanged()
    {
        var shell = new TagManagerShell();
        var changeCount = 0;
        shell.OpenChanged += (_, _) => changeCount++;

        shell.Open();
        shell.Close();

        await Assert.That(changeCount).IsEqualTo(2);
        await Assert.That(shell.IsOpen).IsFalse();
    }
}
