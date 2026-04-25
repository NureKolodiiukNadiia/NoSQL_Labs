using MongoDB.Bson;
using SpotRent.Enums;

namespace SpotRent.Dto;

public record CreateBookingRequest(
    ObjectId WorkspaceId,
    ObjectId UserId,
    DateTime StartTime,
    DateTime EndTime,
    decimal TotalAmount,
    BookingStatus Status
);