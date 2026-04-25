using MongoDB.Bson;

namespace SpotRent.Interfaces;

public record UserSpendingDto(
    ObjectId Id,
    string? FirstName,
    string? LastName,
    string Email,
    decimal TotalSpent,
    int TotalBookings
);
