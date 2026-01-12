namespace mark.davison.rome.web.services.State;

public interface IStateService
{
    event EventHandler StateChanged;
    void NotifyStateChanged();
    bool Loading { get; }
    bool Loaded { get; }
}
