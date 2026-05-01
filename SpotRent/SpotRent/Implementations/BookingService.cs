using MongoDB.Bson;
using MongoDB.Driver;
using SpotRent.Entities;
using SpotRent.Interfaces;
using SpotRent.Data;
using SpotRent.Dto;
using SpotRent.Models;

namespace SpotRent.Implementations;

public sealed class BookingService : IBookingService
{
    private readonly IMongoCollection<Booking> _bookings;
    private readonly IMongoCollection<User> _users;

    public BookingService(MongoDbContext db)
    {
        _bookings = db.Bookings;
        _users = db.Users;
    }

    public async Task<Result> CreateBookingAsync(CreateBookingDto dto, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var booking = new Booking(ObjectId.GenerateNewId(), dto.WorkspaceId, dto.UserId, dto.StartTime,
            dto.EndTime, dto.TotalAmount, dto.Status);
        await _bookings.InsertOneAsync(booking, ct);

        return Result.Success();
    }

    public async Task<Result<IEnumerable<BookingDto>>> GetBookingsAsync(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var bookings = await _bookings.Find(FilterDefinition<Booking>.Empty)
            .Project(b => new BookingDto(b.Id.ToString(), b.WorkspaceId.ToString(), b.UserId.ToString(), b.StartTime, b.EndTime, b.TotalAmount, b.Status))
            .ToListAsync(ct);
        return Result.Success<IEnumerable<BookingDto>>(bookings);
    }

    public async Task<Result<BookingDto>> GetBookingAsync(ObjectId id, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var booking = await _bookings.Find(b => b.Id == id)
            .Project(b => new BookingDto(b.Id.ToString(), b.WorkspaceId.ToString(), b.UserId.ToString(), b.StartTime, b.EndTime, b.TotalAmount, b.Status))
            .FirstOrDefaultAsync(ct);
        if (booking is null)
        {
            return Result.Fail<BookingDto>("Booking not found");
        }

        return Result.Success(booking);
    }

    public async Task<Result> UpdateBookingAsync(ObjectId id, CreateBookingDto dto, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var update = Builders<Booking>.Update
            .Set(b => b.WorkspaceId, dto.WorkspaceId)
            .Set(b => b.UserId, dto.UserId)
            .Set(b => b.StartTime, dto.StartTime)
            .Set(b => b.EndTime, dto.EndTime)
            .Set(b => b.TotalAmount, dto.TotalAmount)
            .Set(b => b.Status, dto.Status);

        var result = await _bookings.UpdateOneAsync(b => b.Id == id, update, cancellationToken: ct);
        if (result.MatchedCount == 0)
        {
            return Result.Fail("Booking not found");
        }

        return Result.Success();
    }

    public async Task<Result> DeleteBookingAsync(ObjectId id, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var result = await _bookings.DeleteOneAsync(b => b.Id == id, ct);
        if (result.DeletedCount == 0)
        {
            return Result.Fail("Booking not found");
        }

        return Result.Success();
    }

    public async Task<Result<IEnumerable<BookingCountDto>>> GetUserBookingCountsAsync(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var aggregate = await _users.Aggregate()
            .AppendStage<BsonDocument>(new BsonDocument("$lookup",
                new BsonDocument
                {
                    { "from", "bookings" },
                    { "let", new BsonDocument("userId", "$_id") },
                    {
                        "pipeline", new BsonArray
                        {
                            new BsonDocument("$match",
                                new BsonDocument("$expr",
                                    new BsonDocument("$eq", new BsonArray { "$UserId", "$$userId" })
                                )
                            ),
                            new BsonDocument("$count", "count")
                        }
                    },
                    { "as", "booking_meta" }
                }
            ))
            .Project(new BsonDocument
            {
                { "Email", 1 },
                {
                    "bookings_count",
                    new BsonDocument("$ifNull", new BsonArray
                    {
                        new BsonDocument("$arrayElemAt", new BsonArray { "$booking_meta.count", 0 }),
                        0
                    })
                }
            })
            .ToListAsync(ct);

        var dtos = aggregate.Select(x => new BookingCountDto(
            x.GetValue("Email", string.Empty).AsString,
            x.GetValue("bookings_count", 0).AsInt32
        ));

        return Result.Success(dtos);
    }
}
