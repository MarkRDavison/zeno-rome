namespace mark.davison.rome.api.commands.Scenarios.CreateTransaction.Validation;

public interface ICreateTransactionValidatorStrategy
{
    Task ValidateTransactionGroup(CreateTransactionCommandRequest request, CreateTransactionCommandResponse response, ICreateTransctionValidationContext validationContext);
    Task ValidateTranasction(CreateTransactionDto transaction, CreateTransactionCommandResponse response, ICreateTransctionValidationContext validationContext);
}