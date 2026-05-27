using Skaldling.Api.Infrastructure.Endpoints;

namespace Skaldling.Api.Features.Heroes.UpdateAvatar;

public static class UpdateAvatarEndpoint
{
    public static IEndpointRouteBuilder MapUpdateAvatar(this IEndpointRouteBuilder routes)
    {
        routes.MapPut("/api/heroes/{id:guid}/avatar", async (
                Guid id,
                UpdateAvatarCommand command,
                UpdateAvatarHandler handler,
                CancellationToken cancellationToken) =>
            {
                var (outcome, response) = await handler.HandleAsync(id, command, cancellationToken);
                return outcome switch
                {
                    UpdateAvatarHandler.Outcome.Updated => Results.Ok(response),
                    UpdateAvatarHandler.Outcome.HeroNotFound => Results.NotFound(),
                    UpdateAvatarHandler.Outcome.UnknownSpriteIds =>
                        Results.Problem(title: "One or more sprite IDs are not found in the catalog.", statusCode: 422),
                    UpdateAvatarHandler.Outcome.DuplicateTypes =>
                        Results.Problem(
                            title: "Hero avatar selection cannot include two or more sprites of the same type.",
                            statusCode: 422),
                    UpdateAvatarHandler.Outcome.MissingTypes =>
                        Results.Problem(title: "Hero avatar must include one sprite per type.", statusCode: 422),
                    _ => Results.StatusCode(500)
                };
            })
            .AddEndpointFilter<ValidationFilter<UpdateAvatarCommand>>();

        return routes;
    }
}