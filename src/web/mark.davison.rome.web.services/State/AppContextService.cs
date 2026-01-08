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

    public AppContextState? GetChangedState(AppContextState existing)
    {
        if (State.RangeStart != existing.RangeStart ||
            State.RangeEnd != existing.RangeEnd)
        {
            Console.WriteLine("GetChangedState - changed!");
            return State;
        }

        Console.WriteLine("GetChangedState - not changed");
        Console.WriteLine(" - start: {0} vs {1}", State.RangeStart, existing.RangeStart);
        Console.WriteLine(" - end:   {0} vs {1}", State.RangeEnd, existing.RangeEnd);
        return null;
    }
}
