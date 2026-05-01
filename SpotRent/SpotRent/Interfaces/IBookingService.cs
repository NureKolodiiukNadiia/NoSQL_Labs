using SpotRent.Dto;
using MongoDB.Bson;
using SpotRent.Implementations;
using SpotRent.Models;

namespace SpotRent.Interfaces;

public interface IBookingService
{
    Task<Result> CreateBookingAsync(CreateBookingDto dto, CancellationToken ct = default);
    Task<Result<IEnumerable<BookingDto>>> GetBookingsAsync(CancellationToken ct = default);
    Task<Result<BookingDto>> GetBookingAsync(ObjectId id, CancellationToken ct = default);
    Task<Result> UpdateBookingAsync(ObjectId id, CreateBookingDto dto, CancellationToken ct = default);
    Task<Result> DeleteBookingAsync(ObjectId id, CancellationToken ct = default);
    Task<Result<IEnumerable<BookingCountDto>>> GetUserBookingCountsAsync(CancellationToken ct = default);
}
