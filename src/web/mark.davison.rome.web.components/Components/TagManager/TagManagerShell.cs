namespace mark.davison.rome.web.components.Components.TagManager;

internal sealed class TagManagerShell : ITagManagerShell
{
    public bool IsOpen { get; private set; }

    public event EventHandler? OpenChanged;

    public event EventHandler? Closed;

    public void Open()
    {
        if (IsOpen)
        {
            return;
        }

        IsOpen = true;
        OpenChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Close()
    {
        if (!IsOpen)
        {
            return;
        }

        IsOpen = false;
        OpenChanged?.Invoke(this, EventArgs.Empty);
        Closed?.Invoke(this, EventArgs.Empty);
    }
}
