namespace mark.davison.rome.web.components.Layouts;

public partial class NavMenu
{
    [Inject]
    public required ITagManagerShell TagManagerShell { get; set; }

    private void OpenTags()
    {
        TagManagerShell.Open();
    }
}
