using MongoDB.Bson;
using SpotRent.Implementations;
using SpotRent.Models;

namespace SpotRent.Interfaces;

public interface IBookingService
{
    Task<Result> CreateBookingAsync(ObjectId userId, ObjectId workspaceId, CancellationToken ct = default);
    Task<Result<IEnumerable<BookingCountDto>>> GetUserBookingCountsAsync(CancellationToken ct = default);
}
