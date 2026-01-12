namespace mark.davison.rome.api.commands.Scenarios.UpsertAccount;

public sealed class UpsertAccountCommandValidator : ICommandValidator<UpsertAccountCommandRequest, UpsertAccountCommandResponse>
{
    public const string VALIDATION_ACCOUNT_TYPE_ID = "INVALID_ACCOUNT_TYPE_ID";
    public const string VALIDATION_CURRENCY_ID = "INVALID_CURRENCY_ID";
    public const string VALIDATION_MISSING_REQ_FIELD = "MISSING_REQ${0}";
    public const string VALIDATION_DUPLICATE_ACC_NUM = "DUPLICATE_ACC_NUM";
    public const string VALIDATION_MISSING_OPENING_BAL_DATE = "MISSING_OPENING_BAL_DATE";

    private readonly IDbContext<RomeDbContext> _dbContext;

    public UpsertAccountCommandValidator(IDbContext<RomeDbContext> dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UpsertAccountCommandResponse> ValidateAsync(UpsertAccountCommandRequest request, ICurrentUserContext currentUserContext, CancellationToken cancellationToken)
    {
        var response = new UpsertAccountCommandResponse();

        var accountTypeExists = await _dbContext.ExistsAsync<AccountType>(request.UpsertAccountDto.AccountTypeId, cancellationToken);

        if (!accountTypeExists)
        {
            response.Errors.Add(VALIDATION_ACCOUNT_TYPE_ID);
            return response;
        }

        var currencyExists = await _dbContext.ExistsAsync<Currency>(request.UpsertAccountDto.CurrencyId, cancellationToken);

        if (!currencyExists)
        {
            response.Errors.Add(VALIDATION_CURRENCY_ID);
            return response;
        }

        if (string.IsNullOrEmpty(request.UpsertAccountDto.Name))
        {
            response.Errors.Add(string.Format(VALIDATION_MISSING_REQ_FIELD, nameof(Account.Name)));
            return response;
        }

        if (!await ValidateDuplicateAccount(request, currentUserContext, cancellationToken))
        {
            response.Errors.Add(VALIDATION_DUPLICATE_ACC_NUM);
            return response;
        }

        if (request.UpsertAccountDto.OpeningBalance != null && request.UpsertAccountDto.OpeningBalanceDate == null)
        {
            response.Errors.Add(VALIDATION_MISSING_OPENING_BAL_DATE);
            return response;
        }

        return response;
    }

    internal async Task<bool> ValidateDuplicateAccount(UpsertAccountCommandRequest request, ICurrentUserContext currentUserContext, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.UpsertAccountDto.AccountNumber))
        {
            return true;
        }

        Guid opposingGuid = Guid.Empty;
        if (request.UpsertAccountDto.AccountTypeId == AccountTypeConstants.Expense)
        {
            opposingGuid = AccountTypeConstants.Revenue;
        }
        else if (request.UpsertAccountDto.AccountTypeId == AccountTypeConstants.Revenue)
        {
            opposingGuid = AccountTypeConstants.Expense;
        }

        var duplicateAccounts = await _dbContext
            .Set<Account>()
            .AsNoTracking()
            .Where(_ => _.UserId == currentUserContext.UserId)
            .Select(_ => new { _.Id, _.AccountNumber, _.AccountTypeId })
            .ToListAsync(cancellationToken);

        foreach (var duplicateAccount in duplicateAccounts
            .Where(_ =>
                _.Id != request.UpsertAccountDto.Id &&
                _.AccountNumber == request.UpsertAccountDto.AccountNumber))
        {
            if (opposingGuid == Guid.Empty)
            {
                return false;
            }

            if (duplicateAccount.AccountTypeId != opposingGuid)
            {
                return false;
            }
        }

        return true;
    }
}
