namespace mark.davison.rome.api.commands.Scenarios.UpsertAccount;

public sealed class UpsertAccountCommandProcessor : ICommandProcessor<UpsertAccountCommandRequest, UpsertAccountCommandResponse>
{
    private readonly IDateService _dateService;
    private readonly IDbContext<RomeDbContext> _dbContext;
    private readonly ICommandDispatcher _commandDispatcher;

    public UpsertAccountCommandProcessor(
        IDateService dateService,
        IDbContext<RomeDbContext> dbContext,
        ICommandDispatcher commandDispatcher)
    {
        _dateService = dateService;
        _dbContext = dbContext;
        _commandDispatcher = commandDispatcher;
    }

    public async Task<UpsertAccountCommandResponse> ProcessAsync(UpsertAccountCommandRequest request, ICurrentUserContext currentUserContext, CancellationToken cancellationToken)
    {
        using (var transaction = await _dbContext.BeginTransactionAsync(cancellationToken))
        {
            var existingAccount = await _dbContext.GetByIdAsync<Account>(
                request.UpsertAccountDto.Id,
                cancellationToken);

            var existingOpeningBalanceTransactionJournal = await _dbContext
                .Set<TransactionJournal>()
                .Include(tj => tj.Transactions)
                .Where(tj =>
                    tj.TransactionTypeId == TransactionTypeConstants.OpeningBalance &&
                    tj.Transactions.Any(t =>
                        t.AccountId == request.UpsertAccountDto.Id))
                .FirstOrDefaultAsync(cancellationToken);

            Account account;
            if (existingAccount is null)
            {
                account = new Account
                {
                    Id = request.UpsertAccountDto.Id,
                    Created = _dateService.Now,
                    LastModified = _dateService.Now,
                    IsActive = true,
                    Order = -1,
                    UserId = currentUserContext.UserId
                };
            }
            else
            {
                account = existingAccount;
            }

            account.Name = request.UpsertAccountDto.Name;
            account.VirtualBalance = request.UpsertAccountDto.VirtualBalance;
            account.AccountNumber = request.UpsertAccountDto.AccountNumber;
            account.AccountTypeId = request.UpsertAccountDto.AccountTypeId;
            account.CurrencyId = request.UpsertAccountDto.CurrencyId;

            await _dbContext.UpsertEntityAsync(account, cancellationToken);

            bool requestHasOpeningBalance =
                request.UpsertAccountDto.OpeningBalance is not null &&
                request.UpsertAccountDto.OpeningBalanceDate is not null;

            bool openingBalanceNeedsEditing = requestHasOpeningBalance && existingOpeningBalanceTransactionJournal is not null;
            bool openingBalanceNeedsDeleting = !requestHasOpeningBalance && existingOpeningBalanceTransactionJournal is not null;

            if (requestHasOpeningBalance && existingOpeningBalanceTransactionJournal == null)
            {
                var createTransactionRequest = new CreateTransactionCommandRequest()
                {
                    TransactionTypeId = TransactionTypeConstants.OpeningBalance,
                    Transactions =
                    {
                        new CreateTransactionDto(
                            Guid.NewGuid(),
                            BuiltinAccountNames.GetBuiltinAccountName(AccountConstants.OpeningBalance),
                            AccountConstants.OpeningBalance,
                            account.Id,
                            request.UpsertAccountDto.OpeningBalanceDate!.Value,
                            request.UpsertAccountDto.OpeningBalance!.Value,
                            null,
                            account.CurrencyId,
                            null)
                    }
                };
                var createTransactionResponse = await _commandDispatcher.Dispatch<CreateTransactionCommandRequest, CreateTransactionCommandResponse>(createTransactionRequest, cancellationToken);

                if (!createTransactionResponse.Success)
                {
                    throw new NotImplementedException("TODO: HANDLE INVALID CREATE OPENING BALANCE");
                }
            }
            else if (openingBalanceNeedsEditing || openingBalanceNeedsDeleting)
            {
                if (openingBalanceNeedsEditing)
                {
                    var sourceAccountTransaction = existingOpeningBalanceTransactionJournal!
                            .Transactions
                            .First(_ => _.AccountId == AccountConstants.OpeningBalance);
                    var destinationAccountTransaction = existingOpeningBalanceTransactionJournal!
                            .Transactions
                            .First(_ => _.AccountId == account.Id);

                    if (sourceAccountTransaction.Amount != -request.UpsertAccountDto.OpeningBalance!.Value ||
                        destinationAccountTransaction.Amount != +request.UpsertAccountDto.OpeningBalance!.Value ||
                        existingOpeningBalanceTransactionJournal!.Date != request.UpsertAccountDto.OpeningBalanceDate!.Value)
                    {
                        sourceAccountTransaction.Amount = -request.UpsertAccountDto.OpeningBalance!.Value;
                        destinationAccountTransaction.Amount = +request.UpsertAccountDto.OpeningBalance!.Value;
                        existingOpeningBalanceTransactionJournal.Date = request.UpsertAccountDto.OpeningBalanceDate!.Value;

                        await _dbContext.UpsertEntitiesAsync<Transaction>(
                        [
                            sourceAccountTransaction,
                            destinationAccountTransaction
                        ], cancellationToken);

                        await _dbContext.UpsertEntityAsync(
                            existingOpeningBalanceTransactionJournal!,
                            cancellationToken);
                    }

                }
                else if (openingBalanceNeedsDeleting)
                {
                    await _dbContext.DeleteEntitiesByIdAsync<Transaction>(
                        [.. existingOpeningBalanceTransactionJournal!.Transactions.Select(_ => _.Id)],
                        cancellationToken);
                    await _dbContext.DeleteEntityByIdAsync<TransactionJournal>(
                        existingOpeningBalanceTransactionJournal!.Id,
                        cancellationToken);
                    await _dbContext.DeleteEntityByIdAsync<TransactionGroup>(
                        existingOpeningBalanceTransactionJournal!.TransactionGroupId,
                        cancellationToken);
                }
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            await transaction.CommitTransactionAsync(cancellationToken);

            var response = new UpsertAccountCommandResponse
            {
                // TODO: Helper
                Value = new AccountDto(
                    account.Id,
                    account.Name,
                    account.AccountTypeId,
                    account.AccountNumber,
                    request.UpsertAccountDto.OpeningBalance ?? 0,
                    account.IsActive,
                    account.LastModified,
                    request.UpsertAccountDto.OpeningBalance ?? 0,
                    account.CurrencyId,
                    account.VirtualBalance,
                    request.UpsertAccountDto.OpeningBalance ?? 0,
                    request.UpsertAccountDto.OpeningBalanceDate)
            };

            return response;
        }
    }
}
