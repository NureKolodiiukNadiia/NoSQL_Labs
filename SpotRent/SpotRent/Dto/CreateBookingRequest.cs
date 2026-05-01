using MongoDB.Bson;
using SpotRent.Enums;

namespace SpotRent.Dto;

public record CreateBookingRequest(
    string WorkspaceId,
    string UserId,
    DateTime StartTime,
    DateTime EndTime,
    decimal TotalAmount,
    BookingStatus Status
);

public record CreateBookingDto(
    ObjectId WorkspaceId,
    ObjectId UserId,
    DateTime StartTime,
    DateTime EndTime,
    decimal TotalAmount,
    BookingStatus Status);