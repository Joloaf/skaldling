using Skaldling.Api.Infrastructure.Endpoints;

namespace Skaldling.Api.Features.Adventures.CreateAdventure;

public static class CreateAdventureEndpoint
{
    public static IEndpointRouteBuilder MapCreateAdventure(this IEndpointRouteBuilder routes)
    {
        routes.MapPost("/api/adventures", async (
                CreateAdventureCommand command,
                CreateAdventureHandler handler,
                CancellationToken cancellationToken) =>
            {
                var (outcome, response) = await handler.HandleAsync(command, cancellationToken);
                return outcome switch
                {
                    CreateAdventureHandler.Outcome.AdventureCreated =>
                        Results.Created($"/api/adventures/{response!.Id}", response),
                    CreateAdventureHandler.Outcome.HeroNotFound =>
                        Results.NotFound(),
                    CreateAdventureHandler.Outcome.ThemeNotFound =>
                        Results.Problem(title: "The selected theme does not exist.", statusCode: 422),
                    CreateAdventureHandler.Outcome.HeroAlreadyOnAdventure =>
                        Results.Problem(
                            title: "This hero is already on an adventure. Complete or abandon it before starting a new one.",
                            statusCode: 409),
                    _ => Results.StatusCode(500)
                };
            })
            .AddEndpointFilter<ValidationFilter<CreateAdventureCommand>>();

        return routes;
    }
}