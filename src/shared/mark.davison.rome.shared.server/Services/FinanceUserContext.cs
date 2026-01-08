using mark.davison.common.abstractions.Services;
using mark.davison.common.authentication.server.abstractions.Services;
using mark.davison.rome.shared.accounting.rules;
using Microsoft.Extensions.Caching.Distributed;

namespace mark.davison.rome.shared.server.Services;

internal sealed class FinanceUserContext : IFinanceUserContext
{
    private bool _loaded;
    private readonly IDistributedCache _distributedCache;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IDateService _dateService;

    private string KeyPrefix => $"FUC_{_currentUserContext.UserId}_{_currentUserContext.TenantId}";
    private string Key(string name) => KeyPrefix + "_" + name;

    public FinanceUserContext(
        IDistributedCache distributedCache,
        ICurrentUserContext currentUserContext,
        IDateService dateService)
    {
        _distributedCache = distributedCache;
        _currentUserContext = currentUserContext;
        _dateService = dateService;
    }

    public DateOnly RangeStart { get; private set; }
    public DateOnly RangeEnd { get; private set; }

    public async Task LoadAsync(CancellationToken cancellationToken)
    {
        if (_loaded)
        {
            return;
        }

        // TODO: Store as serialised state rather than 2 cache entries
        var rangeStart = await _distributedCache.GetStringAsync(Key(nameof(RangeStart)), cancellationToken);
        var rangeEnd = await _distributedCache.GetStringAsync(Key(nameof(RangeEnd)), cancellationToken);

        if (DateOnly.TryParse(rangeStart, out var start) &&
            DateOnly.TryParse(rangeEnd, out var end))
        {
            RangeStart = start;
            RangeEnd = end;
        }
        else
        {
            await ResetAsync(cancellationToken);
        }

        _loaded = true;
    }

    public async Task ResetAsync(CancellationToken cancellationToken)
    {
        var (start, end) = DateRules.GetMonthRange(_dateService.Today);
        await SetAsync(start, end, cancellationToken);
    }

    public async Task SetAsync(DateOnly rangeStart, DateOnly rangeEnd, CancellationToken cancellationToken)
    {
        RangeStart = rangeStart;
        RangeEnd = rangeEnd;

        await _distributedCache.SetStringAsync(Key(nameof(RangeStart)), RangeStart.ToString(), cancellationToken);
        await _distributedCache.SetStringAsync(Key(nameof(RangeEnd)), RangeEnd.ToString(), cancellationToken);
    }
}
