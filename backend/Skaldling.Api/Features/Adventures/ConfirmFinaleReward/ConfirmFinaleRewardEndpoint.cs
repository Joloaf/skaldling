namespace Skaldling.Api.Features.Adventures.ConfirmFinaleReward;

public static class ConfirmFinaleRewardEndpoint
{
    public static IEndpointRouteBuilder MapConfirmFinaleReward(this IEndpointRouteBuilder routes)
    {
        routes.MapPost("/api/adventures/{id:guid}/confirm-finale", async (
            Guid id,
            ConfirmFinaleRewardHandler handler,
            CancellationToken cancellationToken) =>
        {
            var (outcome, response) = await handler.HandleAsync(id, cancellationToken);
            return outcome switch
            {
                ConfirmFinaleRewardHandler.Outcome.FinaleConfirmed => Results.Ok(response),
                ConfirmFinaleRewardHandler.Outcome.AdventureNotFound => Results.NotFound(),
                ConfirmFinaleRewardHandler.Outcome.NotActive =>
                    Results.Problem(
                        title: "Finale can only be confirmed while the adventure is Active.",
                        statusCode: 409),
                _ => Results.StatusCode(500)
            };
        });

        return routes;
    }
}