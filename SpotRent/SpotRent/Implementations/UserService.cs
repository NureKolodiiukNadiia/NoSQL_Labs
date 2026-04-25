using MongoDB.Bson;
using MongoDB.Driver;
using SpotRent.Entities;
using SpotRent.Interfaces;
using SpotRent.Data;
using SpotRent.Dto;
using SpotRent.Models;

namespace SpotRent.Implementations;

public sealed class UserService : IUserService
{
    private readonly IMongoCollection<User> _users;

    public UserService(MongoDbContext db)
    {
        _users = db.Users;
    }

    public async Task<Result> CreateUserAsync(CreateUserRequest request, CancellationToken ct)
    {
        if (await _users
                .Find(u => u.Id == (request.Id ?? ObjectId.Empty) || u.Email == request.Email)
                .AnyAsync(ct))
        {
            return Result.Fail("User exists");
        }

        var user = new User(
            request.Id ?? ObjectId.GenerateNewId(),
            request.Email,
            request.Password,
            request.FirstName,
            request.LastName);
        await _users.InsertOneAsync(user, ct);

        return Result.Success();
    }

    public async Task<Result<UserDto>> GetUserAsync(ObjectId id, CancellationToken ct)
    {
        var userDto = await _users
            .Find(u => u.Id == id)
            .Project(u => new UserDto(u.Id, u.Email, u.FirstName, u.LastName))
            .FirstOrDefaultAsync(ct);
        if (userDto is null)
        {
            return Result.Fail<UserDto>("Not found");
        }

        return Result.Success(userDto);
    }
}
