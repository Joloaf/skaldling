using FluentValidation;

namespace Skaldling.Api.Features.Heroes.CreateHero;

public static class CreateHeroEndpoint
{
    public static IEndpointRouteBuilder MapCreateHero(this IEndpointRouteBuilder routes)
    {
        routes.MapPost("/api/heroes", async (
            CreateHeroCommand command,
            IValidator<CreateHeroCommand> validator,
            CreateHeroHandler handler,
            CancellationToken cancellationToken) =>
        {
            var validation = await validator.ValidateAsync(command, cancellationToken);
            if (!validation.IsValid)
            {
                return Results.ValidationProblem(validation.ToDictionary());
            }

            var response = await handler.HandleAsync(command, cancellationToken);
            return Results.Created($"/api/heroes/{response.Id}", response);
        });

        return routes;
    }
}