namespace mark.davison.rome.web.components.CommonCandidates;

public abstract class BaseViewModel<TPayload> : INotifyPropertyChanged, IDisposable
{
    private readonly IList<Action> _stateUnsubscribers = [];
    private bool disposedValue;

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void RegisterState<TStateService>(TStateService state)
        where TStateService : IStateService
    {
        state.StateChanged += State_StateChanged;

        _stateUnsubscribers.Add(() => state.StateChanged -= State_StateChanged);
    }

    public virtual Task<bool> Initialize(TPayload payload)
    {
        return Task.FromResult(true);
    }

    private void State_StateChanged(object? sender, EventArgs e)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                foreach (var action in _stateUnsubscribers)
                {
                    action();
                }

                _stateUnsubscribers.Clear();
            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
