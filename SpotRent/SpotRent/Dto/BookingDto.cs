using SpotRent.Enums;

namespace SpotRent.Dto;

public record BookingDto(
    string Id,
    string WorkspaceId,
    string UserId,
    DateTime StartTime,
    DateTime EndTime,
    decimal TotalAmount,
    BookingStatus Status
);
