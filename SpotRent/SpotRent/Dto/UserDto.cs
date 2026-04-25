using MongoDB.Bson;

namespace SpotRent.Dto;

public record UserDto(
    ObjectId Id,
    string Email,
    string? FirstName,
    string? LastName
);
