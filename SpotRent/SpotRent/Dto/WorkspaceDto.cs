using MongoDB.Bson;
using SpotRent.Enums;

namespace SpotRent.Dto;

public record WorkspaceDto(
    ObjectId Id,
    string Name,
    SpaceType SpaceType,
    decimal HourlyRate,
    int? Capacity,
    IEnumerable<string>? Amenities,
    Location? Location,
    bool? IsActive
);

public record Location(double Longitude, double Latitude);
