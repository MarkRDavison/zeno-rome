namespace mark.davison.rome.web.components.tests;

internal sealed class RomeComponentTestContext : Bunit.TestContext
{
    public RomeComponentTestContext()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddMudServices();
    }

    public IRenderedComponent<TComponent> RenderComponentWithPopoverProvider<TComponent>(
        Action<ComponentParameterCollectionBuilder<TComponent>>? parameterBuilder = null)
        where TComponent : IComponent
    {
        var fragment = Render(builder =>
        {
            builder.OpenComponent<MudPopoverProvider>(0);
            builder.CloseComponent();
            builder.OpenComponent<TComponent>(1);
            if (parameterBuilder is not null)
            {
                var parameters = new ComponentParameterCollectionBuilder<TComponent>();
                parameterBuilder(parameters);
                foreach (var parameter in parameters.Build())
                {
                    builder.AddAttribute(2, parameter.Name, parameter.Value);
                }
            }

            builder.CloseComponent();
        });

        return fragment.FindComponent<TComponent>();
    }
}
