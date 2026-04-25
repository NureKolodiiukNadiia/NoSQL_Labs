using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver.GeoJsonObjectModel;
using SpotRent.Enums;

namespace SpotRent.Entities;

public record WorkSpace
{
    public WorkSpace(ObjectId Id,
        string Name,
        SpaceType SpaceType,
        decimal HourlyRate,
        int? Capacity = null,
        IEnumerable<string>? Amenities = null,
        GeoJsonPoint<GeoJson2DCoordinates>? Location = null,
        bool? IsActive = null)
    {
        this.Id = Id;
        this.Name = Name;
        this.SpaceType = SpaceType;
        this.HourlyRate = HourlyRate;
        this.Capacity = Capacity;
        this.Amenities = Amenities;
        this.Location = Location;
        this.IsActive = IsActive;
    }

    public ObjectId Id { get; init; }
    public string Name { get; init; }
    public SpaceType SpaceType { get; init; }
    public decimal HourlyRate { get; init; }
    [BsonIgnoreIfNull]
    public int? Capacity { get; init; }
    [BsonIgnoreIfNull]
    public IEnumerable<string>? Amenities { get; init; }
    [BsonIgnoreIfNull]
    public GeoJsonPoint<GeoJson2DCoordinates>? Location { get; init; }
    [BsonIgnoreIfNull]
    public bool? IsActive { get; init; }
}
