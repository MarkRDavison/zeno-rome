namespace mark.davison.rome.web.components.Components.TagManager;

public interface ITagManagerShell
{
    bool IsOpen { get; }

    event EventHandler? OpenChanged;

    event EventHandler? Closed;

    void Open();

    void Close();
}
