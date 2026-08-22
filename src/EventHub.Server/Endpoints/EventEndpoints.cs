using EventHub.Application.Events;

namespace EventHub.Server.Endpoints;

public static class EventEndpoints
{
    public static IEndpointRouteBuilder MapEventEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/events");

        group.MapPost("/", async (
            CreateEventRequest request,
            EventService eventService,
            CancellationToken cancellationToken) =>
        {
            var result = await eventService.CreateAsync(
                new CreateEventCommand(request.Name, request.EventDateUtc),
                cancellationToken);
            return Results.Created($"/api/v1/events/{result.Event.Id}", result);
        });

        group.MapGet("/{eventId:guid}", async (
            Guid eventId,
            EventService eventService,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await eventService.GetAsync(eventId, cancellationToken));
        });

        return endpoints;
    }

    public sealed record CreateEventRequest(string Name, DateTimeOffset EventDateUtc);
}
