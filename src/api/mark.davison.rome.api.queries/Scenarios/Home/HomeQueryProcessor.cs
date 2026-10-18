using mark.davison.rome.api.persistence.UserPreferences;
using mark.davison.rome.shared.models.dto.Scenarios.Queries.Home;

namespace mark.davison.rome.api.queries.Scenarios.Home;

public sealed class HomeQueryProcessor : IQueryProcessor<HomeQueryRequest, HomeQueryResponse>
{
    private readonly IDbContext<RomeDbContext> _dbContext;
    private readonly IFinanceUserContext _financeUserContext;

    public HomeQueryProcessor(
        IDbContext<RomeDbContext> dbContext,
        IFinanceUserContext financeUserContext)
    {
        _dbContext = dbContext;
        _financeUserContext = financeUserContext;
    }

    public async Task<HomeQueryResponse> ProcessAsync(
        HomeQueryRequest request,
        ICurrentUserContext currentUserContext,
        CancellationToken cancellationToken)
    {
        var preferences = await UserPreferencesSupport.LoadAsync(
            _dbContext,
            currentUserContext.UserId,
            cancellationToken);

        var accounts = await LoadHomeAccountsAsync(currentUserContext.UserId, cancellationToken);
        var recent = await LoadRecentTransactionsAsync(
            currentUserContext.UserId,
            preferences.HomeRecentTransactionCount,
            cancellationToken);

        return new HomeQueryResponse
        {
            Value = new HomeQueryValue
            {
                Accounts = accounts,
                RecentTransactions = recent
            }
        };
    }

    private async Task<List<AccountDto>> LoadHomeAccountsAsync(Guid userId, CancellationToken cancellationToken)
    {
        var visibleAccounts = await _dbContext
            .Set<Account>()
            .AsNoTracking()
            .Where(_ => _.UserId == userId)
            .ToListAsync(cancellationToken);

        visibleAccounts = visibleAccounts
            .Where(_ => AccountQuerySupport.IsUserVisibleAccount(_.Id))
            .ToList();

        var useFavouritesOnly = visibleAccounts.Any(_ => _.IsFavourite);
        var filtered = useFavouritesOnly
            ? visibleAccounts.Where(_ => _.IsFavourite)
            : visibleAccounts.Where(_ => _.AccountTypeId == AccountTypeConstants.Asset);

        var ordered = filtered
            .OrderBy(_ => _.Order)
            .ThenBy(_ => _.Name)
            .ToList();

        var accountIds = ordered.Select(_ => _.Id).ToList();
        var openingBalances = await AccountQuerySupport.LoadOpeningBalanceJournalsAsync(
            _dbContext,
            accountIds,
            cancellationToken);

        var result = new List<AccountDto>();
        foreach (var account in ordered)
        {
            var openingBalanceTransactionJournal = openingBalances
                .FirstOrDefault(_ => _.Transactions.Any(t => t.AccountId == account.Id));

            var openingBalanceTransaction = openingBalanceTransactionJournal?
                .Transactions
                .FirstOrDefault(_ => _.AccountId == account.Id);

            var amounts = await AccountQuerySupport.LoadTransactionAmountsAsync(
                _dbContext,
                account.Id,
                cancellationToken);

            result.Add(AccountQuerySupport.ToAccountDto(
                account,
                openingBalanceTransactionJournal,
                openingBalanceTransaction,
                amounts,
                _financeUserContext));
        }

        return result;
    }

    private async Task<List<HomeRecentTransactionDto>> LoadRecentTransactionsAsync(
        Guid userId,
        int limit,
        CancellationToken cancellationToken)
    {
        var rangeStart = _financeUserContext.RangeStart;
        var rangeEnd = _financeUserContext.RangeEnd;

        var journalsInRange = await _dbContext
            .Set<TransactionJournal>()
            .AsNoTracking()
            .Where(_ =>
                _.UserId == userId &&
                _.Date >= rangeStart &&
                _.Date <= rangeEnd)
            .Select(_ => new
            {
                _.TransactionGroupId,
                _.Date,
                _.TransactionTypeId,
                _.Description
            })
            .ToListAsync(cancellationToken);

        var grouped = journalsInRange
            .GroupBy(_ => _.TransactionGroupId)
            .Select(g =>
            {
                var latest = g.OrderByDescending(_ => _.Date).First();
                return new
                {
                    TransactionGroupId = g.Key,
                    Date = g.Max(_ => _.Date),
                    TransactionTypeId = latest.TransactionTypeId,
                    LatestJournalDescription = latest.Description
                };
            })
            .OrderByDescending(_ => _.Date)
            .ThenByDescending(_ => _.TransactionGroupId)
            .Take(limit)
            .ToList();

        if (grouped.Count == 0)
        {
            return [];
        }

        var groupIds = grouped.Select(_ => _.TransactionGroupId).ToList();
        var groups = await _dbContext
            .Set<TransactionGroup>()
            .AsNoTracking()
            .Where(_ => groupIds.Contains(_.Id))
            .ToDictionaryAsync(_ => _.Id, cancellationToken);

        return grouped
            .Select(_ =>
            {
                var groupTitle = groups.TryGetValue(_.TransactionGroupId, out var group)
                    ? group.Title
                    : string.Empty;
                var title = !string.IsNullOrWhiteSpace(groupTitle)
                    ? groupTitle
                    : _.LatestJournalDescription;

                return new HomeRecentTransactionDto(
                    _.TransactionGroupId,
                    _.Date,
                    title,
                    _.TransactionTypeId);
            })
            .ToList();
    }
}
