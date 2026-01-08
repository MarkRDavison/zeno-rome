namespace mark.davison.rome.shared.server.Services;

public interface IFinanceUserContext
{
    DateOnly RangeStart { get; }
    DateOnly RangeEnd { get; }
    Task ResetAsync(CancellationToken cancellationToken);
    Task LoadAsync(CancellationToken cancellationToken);
    Task SetAsync(DateOnly rangeStart, DateOnly rangeEnd, CancellationToken cancellationToken);
}