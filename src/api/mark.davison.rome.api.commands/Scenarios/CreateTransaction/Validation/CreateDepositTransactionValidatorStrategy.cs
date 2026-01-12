namespace mark.davison.rome.api.commands.Scenarios.CreateTransaction.Validation;

internal sealed class CreateDepositTransactionValidatorStrategy : CreateTransactionValidatorStrategy
{
    protected override Guid TransactionTypeId => TransactionTypeConstants.Deposit;
    protected override IEnumerable<Guid> ValidSourceIds => AccountTypeConstants.Revenues;
    protected override IEnumerable<Guid> ValidDestinationIds => AccountTypeConstants.Assets.Concat(AccountTypeConstants.Liabilities);
}