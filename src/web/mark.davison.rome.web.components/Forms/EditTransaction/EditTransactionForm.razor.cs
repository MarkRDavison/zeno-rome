namespace mark.davison.rome.web.components.Forms.EditTransaction;

public partial class EditTransactionForm
{
    [Parameter, EditorRequired]
    public required bool Processing { get; set; }

    private static string Id(string id, int index) => $"{id}-{index}";
}
