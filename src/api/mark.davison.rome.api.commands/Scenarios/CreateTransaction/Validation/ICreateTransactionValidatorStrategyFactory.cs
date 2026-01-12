namespace mark.davison.rome.api.commands.Scenarios.CreateTransaction.Validation;

public interface ICreateTransactionValidatorStrategyFactory
{
    ICreateTransactionValidatorStrategy CreateStrategy(Guid transactionTypeId);
}