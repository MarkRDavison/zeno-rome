namespace mark.davison.rome.shared.models.dto.Scenarios.Commands.CreateTransaction;

public sealed class CreateTransactionCommandResponse : Response
{
    // TODO: --> Create/EditTransactionData payload
    public TransactionGroupDto Group { get; set; } = new(Guid.Empty, string.Empty); // TODO: TransactionGroupDto.Default???
    public List<TransactionJournalDto> Journals { get; set; } = [];
    public List<TransactionDto> Transactions { get; set; } = [];
}