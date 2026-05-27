using Skaldling.Api.Infrastructure.Endpoints;

namespace Skaldling.Api.Features.Heroes.UpdateHeroDetails;

public static class UpdateHeroDetailsEndpoint
{
    public static IEndpointRouteBuilder MapUpdateHeroDetails(this IEndpointRouteBuilder routes)
    {
        routes.MapPut("/api/heroes/{id:guid}/details", async (
                Guid id,
                UpdateHeroDetailsCommand command,
                UpdateHeroDetailsHandler handler,
                CancellationToken cancellationToken) =>
            {
                var (outcome, response) = await handler.HandleAsync(id, command, cancellationToken);
                return outcome switch
                {
                    UpdateHeroDetailsHandler.Outcome.Updated => Results.Ok(response),
                    UpdateHeroDetailsHandler.Outcome.HeroNotFound => Results.NotFound(),
                    _ => Results.StatusCode(500)
                };
            })
            .AddEndpointFilter<ValidationFilter<UpdateHeroDetailsCommand>>();

        return routes;
    }
}