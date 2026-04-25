using MongoDB.Bson;
using SpotRent.Enums;

namespace SpotRent.Dto;

public record BookingDto(
    ObjectId Id,
    ObjectId WorkspaceId,
    ObjectId UserId,
    DateTime StartTime,
    DateTime EndTime,
    decimal TotalAmount,
    BookingStatus Status
);
