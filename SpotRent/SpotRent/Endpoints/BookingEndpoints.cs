using MongoDB.Bson;
using SpotRent.Interfaces;

namespace SpotRent.Endpoints;

public static class BookingEndpoints
{
    public static void MapBookingEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/bookings").WithTags("Bookings");

        group.MapGet("/booking-counts", GetUsersBookingCountsAsync);
        group.MapPost("", CreateBookingAsync);
    }

    private static async Task<IResult> CreateBookingAsync(IBookingService svc, string userId, string workspaceId,
        CancellationToken ct)
    {
        var uIdIsParsed = ObjectId.TryParse(userId, out var uId);
        var wIdIsParsed = ObjectId.TryParse(workspaceId, out var wId);
        if (!uIdIsParsed || !wIdIsParsed)
        {
            return Results.BadRequest();
        }

        var res = await svc.CreateBookingAsync(uId, wId, ct);

        return res.IsSuccess switch
        {
            true => Results.NoContent(),
            _ => Results.BadRequest()
        };
    }

    private static async Task<IResult> GetUsersBookingCountsAsync(IBookingService svc, CancellationToken ct)
    {
        var res = await svc.GetUserBookingCountsAsync(ct);

        return res.IsSuccess switch
        {
            true => Results.Ok(res.Value),
            _ => Results.BadRequest()
        };
    }
}
