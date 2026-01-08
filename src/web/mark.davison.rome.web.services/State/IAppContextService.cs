namespace mark.davison.rome.web.services.State;

public interface IAppContextService
{
    AppContextState State { get; }

    void UpdateRange(DateOnly start, DateOnly end);

    event EventHandler RangeUpdated;

    AppContextState? GetChangedState(AppContextState existing);
}
