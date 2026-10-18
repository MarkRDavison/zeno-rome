namespace mark.davison.rome.web.services.State;

internal sealed class AppContextService : IAppContextService
{
    private readonly IDateService _dateService;

    public AppContextService(IDateService dateService)
    {
        _dateService = dateService;

        var (rangeStart, rangeEnd) = _dateService.Today.GetMonthRange();

        State = new(rangeStart, rangeEnd);
    }

    public AppContextState State { get; private set; }

    public event EventHandler RangeUpdated = default!;

    public void UpdateRange(DateOnly start, DateOnly end)
    {
        State = new AppContextState(start, end);

        RangeUpdated?.Invoke(this, EventArgs.Empty);
    }
}
