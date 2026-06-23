namespace mark.davison.rome.web.components.Forms.EditCategory;

public class EditCategoryFormViewModel : IFormViewModel
{
    private readonly ICategoryState _categoryState;

    public EditCategoryFormViewModel(ICategoryState categoryState)
    {
        _categoryState = categoryState;
    }

    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public bool Valid =>
        !string.IsNullOrEmpty(Name) &&
        !_categoryState.Categories.Any(c => c.Name == Name && Id != c.Id);
}
