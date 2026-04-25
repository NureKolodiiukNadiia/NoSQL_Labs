using MongoDB.Driver.GeoJsonObjectModel;
using SpotRent.Enums;

namespace SpotRent.Dto;

public record CreateWorkspaceRequest(
    string Name,
    SpaceType SpaceType,
    decimal HourlyRate,
    int? Capacity = null,
    IEnumerable<string>? Amenities = null,
    GeoJsonPoint<GeoJson2DCoordinates>? Location = null,
    bool? IsActive = null
);
