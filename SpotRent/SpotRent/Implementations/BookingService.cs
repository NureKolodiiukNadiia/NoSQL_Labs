using MongoDB.Bson;
using MongoDB.Driver;
using SpotRent.Entities;
using SpotRent.Enums;
using SpotRent.Interfaces;
using SpotRent.Data;
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

    public async Task<Result> CreateBookingAsync(ObjectId userId, ObjectId workspaceId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var booking = new Booking(ObjectId.GenerateNewId(), workspaceId, userId, DateTime.UtcNow,
            DateTime.UtcNow.AddHours(2), 50.0m, BookingStatus.Confirmed);
        await _bookings.InsertOneAsync(booking, ct);

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
                    { "let", new BsonDocument("userId", "$Id") },
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
