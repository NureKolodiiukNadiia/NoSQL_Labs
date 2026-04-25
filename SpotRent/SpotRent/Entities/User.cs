
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SpotRent.Entities;

public record User
{
    public User(ObjectId Id,
        string Email,
        string Password,
        string? FirstName = null,
        string? LastName = null)
    {
        this.Id = Id;
        this.Email = Email;
        this.Password = Password;
        this.FirstName = FirstName;
        this.LastName = LastName;
    }

    public ObjectId Id { get; init; }
    public string Email { get; init; }
    public string Password { get; init; }
    [BsonIgnoreIfNull]
    public string? FirstName { get; init; }
    [BsonIgnoreIfNull]
    public string? LastName { get; init; }
}
