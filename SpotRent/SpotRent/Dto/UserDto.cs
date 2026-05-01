using MongoDB.Bson;

namespace SpotRent.Dto;

public record UserDto(
    string Id,
    string Email,
    string Password,
    string? FirstName,
    string? LastName
);
