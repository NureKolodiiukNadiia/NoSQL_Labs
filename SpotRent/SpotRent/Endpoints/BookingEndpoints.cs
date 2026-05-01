using MongoDB.Bson;
using SpotRent.Dto;
using SpotRent.Interfaces;

namespace SpotRent.Endpoints;

public static class BookingEndpoints
{
    public static void MapBookingEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/bookings").WithTags("Bookings");

        group.MapGet("", GetBookingsAsync);
        group.MapGet("{id}", GetBookingByIdAsync);
        group.MapGet("/booking-counts", GetUsersBookingCountsAsync);
        group.MapPost("", CreateBookingAsync);
        group.MapPut("{id}", UpdateBookingAsync);
        group.MapDelete("{id}", DeleteBookingAsync);
    }

    private static async Task<IResult> CreateBookingAsync(IBookingService svc, CreateBookingRequest req,
        CancellationToken ct)
    {
        var uIdIsParsed = ObjectId.TryParse(req.UserId, out var uId);
        var wIdIsParsed = ObjectId.TryParse(req.WorkspaceId, out var wId);
        if (!uIdIsParsed || !wIdIsParsed)
        {
            return Results.BadRequest();
        }

        var res = await svc.CreateBookingAsync(
            new CreateBookingDto(
                wId, uId, req.StartTime, req.EndTime, req.TotalAmount, req.Status),
            ct);

        return res.IsSuccess switch
        {
            true => Results.NoContent(),
            _ => Results.BadRequest()
        };
    }

    private static async Task<IResult> GetBookingsAsync(IBookingService svc, CancellationToken ct)
    {
        var res = await svc.GetBookingsAsync(ct);
        return res.IsSuccess ? Results.Ok(res.Value) : Results.BadRequest(res.Error);
    }

    private static async Task<IResult> GetBookingByIdAsync(IBookingService svc, string id, CancellationToken ct)
    {
        if (!ObjectId.TryParse(id, out var bookingId))
        {
            return Results.BadRequest("Invalid booking id");
        }

        var res = await svc.GetBookingAsync(bookingId, ct);
        return res.IsSuccess ? Results.Ok(res.Value) : Results.BadRequest(res.Error);
    }

    private static async Task<IResult> UpdateBookingAsync(IBookingService svc, string id, CreateBookingRequest req,
        CancellationToken ct)
    {
        var idIsParsed = ObjectId.TryParse(id, out var bookingId);
        var uIdIsParsed = ObjectId.TryParse(req.UserId, out var uId);
        var wIdIsParsed = ObjectId.TryParse(req.WorkspaceId, out var wId);
        if (!idIsParsed || !uIdIsParsed || !wIdIsParsed)
        {
            return Results.BadRequest();
        }

        var res = await svc.UpdateBookingAsync(
            bookingId,
            new CreateBookingDto(wId, uId, req.StartTime, req.EndTime, req.TotalAmount, req.Status),
            ct);

        return res.IsSuccess ? Results.NoContent() : Results.BadRequest(res.Error);
    }

    private static async Task<IResult> DeleteBookingAsync(IBookingService svc, string id, CancellationToken ct)
    {
        if (!ObjectId.TryParse(id, out var bookingId))
        {
            return Results.BadRequest("Invalid booking id");
        }

        var res = await svc.DeleteBookingAsync(bookingId, ct);
        return res.IsSuccess ? Results.NoContent() : Results.BadRequest(res.Error);
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
