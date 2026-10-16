namespace mark.davison.rome.web.components.Forms.EditTransaction;

public partial class EditTransactionForm : IDisposable
{
    [Inject]
    public required ITagManagerShell TagManagerShell { get; set; }

    [Inject]
    public required ITagState TagState { get; set; }

    [Parameter, EditorRequired]
    public required bool Processing { get; set; }

    [Parameter, EditorRequired]
    public required bool Loading { get; set; }

    private static string Id(string id, int index) => $"{id}-{index}";

    protected override void OnInitialized()
    {
        TagManagerShell.Closed += OnTagManagerClosed;
    }

    private void OpenTagManager() => TagManagerShell.Open();

    private void OnTagIdsChanged(int splitIndex, IEnumerable<Guid> values)
    {
        FormViewModel.Items[splitIndex].TagIds = values?.ToList() ?? [];
    }

    private string FormatSelectedTagNames(IReadOnlyList<string> selectedValues)
    {
        if (selectedValues.Count == 0)
        {
            return string.Empty;
        }

        var selectedIds = new HashSet<Guid>();
        foreach (var value in selectedValues)
        {
            if (Guid.TryParse(value, out var id))
            {
                selectedIds.Add(id);
            }
        }

        return string.Join(
            ", ",
            FormViewModel.TagItems
                .Where(tag => selectedIds.Contains(tag.Id))
                .Select(tag => tag.Name));
    }

    private async void OnTagManagerClosed(object? sender, EventArgs e)
    {
        await TagState.FetchState();
        await InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        TagManagerShell.Closed -= OnTagManagerClosed;
    }
}
