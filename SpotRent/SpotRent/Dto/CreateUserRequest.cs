using MongoDB.Bson;

namespace SpotRent.Dto;

public record CreateUserRequest
{
    public CreateUserRequest(string Email,
        string Password,
        ObjectId? Id,
        string? FirstName,
        string? LastName)
    {
        this.Email = Email;
        this.Password = Password;
        this.Id = Id;
        this.FirstName = FirstName;
        this.LastName = LastName;
    }

    public string Email { get; init; }
    public string Password { get; init; }
    public ObjectId? Id { get; set; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
}
