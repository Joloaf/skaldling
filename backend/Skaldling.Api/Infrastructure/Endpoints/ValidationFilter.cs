using FluentValidation;

namespace Skaldling.Api.Infrastructure.Endpoints;

// Validation extracted out from endpoints to the rule of three
public class ValidationFilter<TCommand> : IEndpointFilter
{
    private readonly IValidator<TCommand> _validator;

    public ValidationFilter(IValidator<TCommand> validator) => _validator = validator;

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var command = context.Arguments.OfType<TCommand>().FirstOrDefault();
        if (command is null)
        {
            return await next(context);
        }

        var validation = await _validator.ValidateAsync(command, context.HttpContext.RequestAborted);
        if (!validation.IsValid)
        {
            return Results.ValidationProblem(validation.ToDictionary());
        }

        return await next(context);
    }
}