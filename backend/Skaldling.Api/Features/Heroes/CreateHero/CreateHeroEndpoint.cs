using Skaldling.Api.Infrastructure.Endpoints;

namespace Skaldling.Api.Features.Heroes.CreateHero;

public static class CreateHeroEndpoint
{
    public static IEndpointRouteBuilder MapCreateHero(this IEndpointRouteBuilder routes)
    {
        routes.MapPost("/api/heroes", async (
                CreateHeroCommand command,
                CreateHeroHandler handler,
                CancellationToken cancellationToken) =>
            {
                var (outcome, response) = await handler.HandleAsync(command, cancellationToken);
                return outcome switch
                {
                    CreateHeroHandler.Outcome.HeroCreated =>
                        Results.Created($"/api/heroes/{response!.Id}", response),
                    CreateHeroHandler.Outcome.UnknownSpriteIds =>
                        Results.Problem(title: "One or more sprite IDs are not found in the catalog.", statusCode: 422),
                    CreateHeroHandler.Outcome.DuplicateTypes =>
                        Results.Problem(
                            title: "Hero avatar selection cannot include two or more sprites of the same type.",
                            statusCode: 422),
                    CreateHeroHandler.Outcome.MissingTypes =>
                        Results.Problem(title: "Hero avatar must include one sprite per type.", statusCode: 422),
                    _ => Results.StatusCode(500)
                };
            })
            .AddEndpointFilter<ValidationFilter<CreateHeroCommand>>();

        return routes;
    }
}