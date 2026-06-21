namespace mark.davison.rome.web.components.CommonCandidates;

public abstract class BaseViewModel<TPayload> : INotifyPropertyChanged, IDisposable
{
    private bool disposedValue;
    private readonly IList<Action> _stateUnsubscribers = [];
    private readonly IList<Func<bool>> _stateLoadingFuncs = [];
    private readonly IAppContextService _appContextService;

    public event PropertyChangedEventHandler? PropertyChanged;

    protected BaseViewModel(IAppContextService appContextService)
    {
        _appContextService = appContextService;

        _appContextService.RangeUpdated += OnContextRangeUpdated;
    }

    private void OnContextRangeUpdated(object? sender, EventArgs e)
    {
        _ = OnAppContextUpdated(_appContextService.State);
    }

    protected virtual Task OnAppContextUpdated(AppContextState state) => Task.CompletedTask;

    protected void RegisterState<TStateService>(TStateService state)
        where TStateService : IStateService
    {
        state.StateChanged += State_StateChanged;

        _stateUnsubscribers.Add(() => state.StateChanged -= State_StateChanged);
        _stateLoadingFuncs.Add(() => state.Loading);
    }

    protected bool IsStateLoading => _stateLoadingFuncs.All(f => f.Invoke());

    public virtual Task<bool> Initialize(TPayload payload)
    {
        return Task.FromResult(true);
    }

    protected void State_StateChanged(object? sender, EventArgs e)
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
                _stateLoadingFuncs.Clear();
                _appContextService.RangeUpdated -= OnContextRangeUpdated;
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
