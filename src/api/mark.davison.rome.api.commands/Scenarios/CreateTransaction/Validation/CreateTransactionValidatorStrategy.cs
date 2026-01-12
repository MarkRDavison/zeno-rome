namespace mark.davison.rome.api.commands.Scenarios.CreateTransaction.Validation;

internal abstract class CreateTransactionValidatorStrategy : ICreateTransactionValidatorStrategy
{

    public async Task ValidateTranasction(CreateTransactionDto transaction, CreateTransactionCommandResponse response, ICreateTransctionValidationContext validationContext)
    {
        var sourceAccount = await validationContext.GetAccountById(transaction.SourceAccountId, CancellationToken.None) ?? throw new NotImplementedException("Need to handle creating new accounts?");
        var destinationAccount = await validationContext.GetAccountById(transaction.DestinationAccountId, CancellationToken.None) ?? throw new NotImplementedException("Need to handle creating new accounts?");

        if (!ValidDestinationIds.Any(_ => _.Equals(destinationAccount.AccountTypeId)))
        {
            response.Errors.Add(CreateTransactionCommandValidator.VALIDATION_INVALID_DESTINATION_ACCOUNT_TYPE);
        }

        if (!ValidSourceIds.Any(_ => _.Equals(sourceAccount.AccountTypeId)))
        {
            response.Errors.Add(CreateTransactionCommandValidator.VALIDATION_INVALID_SOURCE_ACCOUNT_TYPE);
        }

        if (!AllowableSourceDestinationAccounts.IsValidSourceAndDestinationAccount(
            TransactionTypeId,
            sourceAccount.AccountTypeId,
            destinationAccount.AccountTypeId))
        {
            response.Errors.Add(CreateTransactionCommandValidator.VALIDATION_INVALID_ACCOUNT_PAIR);
        }
    }

    public Task ValidateTransactionGroup(CreateTransactionCommandRequest request, CreateTransactionCommandResponse response, ICreateTransctionValidationContext validationContext)
    {
        return Task.CompletedTask;
    }

    protected abstract Guid TransactionTypeId { get; }
    protected abstract IEnumerable<Guid> ValidSourceIds { get; }
    protected abstract IEnumerable<Guid> ValidDestinationIds { get; }
}