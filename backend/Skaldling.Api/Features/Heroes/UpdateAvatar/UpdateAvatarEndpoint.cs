using FluentValidation;

namespace Skaldling.Api.Features.Heroes.UpdateAvatar;

public static class UpdateAvatarEndpoint
{
    public static IEndpointRouteBuilder MapUpdateAvatar(this IEndpointRouteBuilder routes)
    {
        routes.MapPut("/api/heroes/{id:guid}/avatar", async (
            Guid id,
            UpdateAvatarCommand command,
            IValidator<UpdateAvatarCommand> validator,
            UpdateAvatarHandler handler,
            CancellationToken cancellationToken) =>
        {
            var validation = await validator.ValidateAsync(command, cancellationToken);
            if (!validation.IsValid)
            {
                return Results.ValidationProblem(validation.ToDictionary());
            }

            var (outcome, response) = await handler.HandleAsync(id, command, cancellationToken);
            return outcome switch
            {
                UpdateAvatarHandler.Outcome.Update => Results.Ok(response),
                UpdateAvatarHandler.Outcome.HeroNotFound => Results.NotFound(),
                UpdateAvatarHandler.Outcome.UnknownSpriteIds =>
                    Results.Problem(title: "One or more sprites are not found in the catalog.", statusCode: 422),
                UpdateAvatarHandler.Outcome.DuplicatedTypes =>
                    Results.Problem(title: "Hero avatar selection cannot include two or more sprites of the same type.",
                        statusCode: 422),
                UpdateAvatarHandler.Outcome.MissingTypes =>
                    Results.Problem(title: "Hero avatar must include one sprite per type.", statusCode: 422),
                _ => Results.StatusCode(500)
            };
        });

        return routes;
    }
}