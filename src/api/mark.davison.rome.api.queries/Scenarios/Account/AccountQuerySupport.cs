namespace mark.davison.rome.api.queries.Scenarios.AccountQueries;

internal static class AccountQuerySupport
{
    internal sealed class DatedTransactionAmount
    {
        public required long Amount { get; init; }
        public required DateOnly Date { get; init; }
    }

    internal static bool IsUserVisibleAccount(Guid accountId) =>
        accountId != AccountConstants.Reconciliation &&
        accountId != AccountConstants.OpeningBalance;

    internal static async Task<List<TransactionJournal>> LoadOpeningBalanceJournalsAsync(
        IDbContext<RomeDbContext> dbContext,
        IReadOnlyList<Guid> accountIds,
        CancellationToken cancellationToken)
    {
        if (accountIds.Count == 0)
        {
            return [];
        }

        return await dbContext
            .Set<TransactionJournal>()
            .AsNoTracking()
            .Include(_ => _.Transactions)
            .Where(_ =>
                _.Transactions.Any(__ => accountIds.Contains(__.AccountId)) &&
                _.TransactionTypeId == TransactionTypeConstants.OpeningBalance)
            .ToListAsync(cancellationToken);
    }

    internal static async Task<List<DatedTransactionAmount>> LoadTransactionAmountsAsync(
        IDbContext<RomeDbContext> dbContext,
        Guid accountId,
        CancellationToken cancellationToken) =>
        await dbContext
            .Set<Transaction>()
            .AsNoTracking()
            .Where(_ => _.AccountId == accountId)
            .Select(_ => new DatedTransactionAmount { Amount = _.Amount, Date = _.TransactionJournal!.Date })
            .ToListAsync(cancellationToken);

    internal static AccountDto ToAccountDto(
        mark.davison.rome.api.models.Entities.Account account,
        TransactionJournal? openingBalanceTransactionJournal,
        Transaction? openingBalanceTransaction,
        IReadOnlyList<DatedTransactionAmount> amounts,
        IFinanceUserContext financeUserContext)
    {
        var balanceAtEnd = amounts
            .Where(_ => _.Date <= financeUserContext.RangeEnd)
            .Sum(_ => _.Amount);

        var balanceAtStart = amounts
            .Where(_ => _.Date < financeUserContext.RangeStart)
            .Sum(_ => _.Amount);

        var currentBalance = balanceAtEnd;
        var balanceDifference = balanceAtEnd - balanceAtStart;

        return new AccountDto(
            account.Id,
            account.Name,
            account.AccountTypeId,
            account.AccountNumber,
            currentBalance,
            account.IsActive,
            account.IsFavourite,
            account.LastModified,
            balanceDifference,
            account.CurrencyId,
            account.VirtualBalance,
            openingBalanceTransaction?.Amount,
            openingBalanceTransactionJournal?.Date);
    }
}
