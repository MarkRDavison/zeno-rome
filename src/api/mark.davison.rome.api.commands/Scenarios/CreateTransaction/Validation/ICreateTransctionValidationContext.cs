namespace mark.davison.rome.api.commands.Scenarios.CreateTransaction.Validation;

public interface ICreateTransctionValidationContext
{
    Task<Account?> GetAccountById(Guid accountId, CancellationToken cancellationToken);

    Task<Category?> GetCategoryById(Guid categoryId, CancellationToken cancellationToken);

    Task<Tag?> GetTagById(Guid tagId, CancellationToken cancellationToken);
}