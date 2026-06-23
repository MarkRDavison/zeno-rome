namespace mark.davison.rome.web.components.Forms.EditCategory;

public class EditCategoryFormSubmission : IFormSubmission<EditCategoryFormViewModel>
{
    private readonly IClientHttpRepository _clientHttpRepository;

    public EditCategoryFormSubmission(
        IClientHttpRepository clientHttpRepository)
    {
        _clientHttpRepository = clientHttpRepository;
    }

    public async Task<Response> Primary(EditCategoryFormViewModel formViewModel)
    {
        if (formViewModel.Id == Guid.Empty)
        {
            formViewModel.Id = Guid.NewGuid();
        }

        var request = new UpsertCategoryCommandRequest
        {
            Id = formViewModel.Id,
            Name = formViewModel.Name
        };

        var response = await _clientHttpRepository.Post<UpsertCategoryCommandRequest, UpsertCategoryCommandResponse>(request, CancellationToken.None);

        return response;
    }
}
