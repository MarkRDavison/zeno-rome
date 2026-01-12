namespace mark.davison.rome.api.commands.Scenarios.CreateTransaction.Validation;

internal sealed class NullCreateTransactionValidatorStrategy : ICreateTransactionValidatorStrategy
{
    public Task ValidateTranasction(CreateTransactionDto transaction, CreateTransactionCommandResponse response, ICreateTransctionValidationContext validationContext)
    {
        return Task.CompletedTask;
    }

    public Task ValidateTransactionGroup(CreateTransactionCommandRequest request, CreateTransactionCommandResponse response, ICreateTransctionValidationContext validationContext)
    {
        return Task.CompletedTask;
    }
}