namespace mark.davison.rome.api.commands.Scenarios.CreateTransaction.Validation;

internal sealed class CreateTransferTransactionValidatorStrategy : CreateTransactionValidatorStrategy
{
    protected override Guid TransactionTypeId => TransactionTypeConstants.Transfer;
    protected override IEnumerable<Guid> ValidSourceIds => AccountTypeConstants.Assets.Concat(AccountTypeConstants.Liabilities);
    protected override IEnumerable<Guid> ValidDestinationIds => AccountTypeConstants.Assets.Concat(AccountTypeConstants.Liabilities);
}