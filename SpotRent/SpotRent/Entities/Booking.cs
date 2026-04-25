using MongoDB.Bson;
using SpotRent.Enums;

namespace SpotRent.Entities;

public sealed record Booking(
    ObjectId Id,
    ObjectId WorkspaceId,
    ObjectId UserId,
    DateTime StartTime,
    DateTime EndTime,
    decimal TotalAmount,
    BookingStatus Status
);
