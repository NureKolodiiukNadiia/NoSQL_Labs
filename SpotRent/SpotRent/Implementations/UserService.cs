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

    public async Task<Result<IEnumerable<UserDto>>> GetUsersAsync(CancellationToken ct)
    {
        var users = await _users
            .Find(FilterDefinition<User>.Empty)
            .Project(u => new UserDto(u.Id.ToString(), u.Email, u.Password, u.FirstName, u.LastName))
            .ToListAsync(ct);
        return Result.Success<IEnumerable<UserDto>>(users);
    }

    public async Task<Result<UserDto>> GetUserAsync(ObjectId id, CancellationToken ct)
    {
        var userDto = await _users
            .Find(u => u.Id == id)
            .Project(u => new UserDto(u.Id.ToString(), u.Email, u.Password, u.FirstName, u.LastName))
            .FirstOrDefaultAsync(ct);
        if (userDto is null)
        {
            return Result.Fail<UserDto>("Not found");
        }

        return Result.Success(userDto);
    }

    public async Task<Result> UpdateUserAsync(ObjectId id, CreateUserRequest request, CancellationToken ct)
    {
        var userWithSameEmail = await _users.Find(u => u.Email == request.Email && u.Id != id).AnyAsync(ct);
        if (userWithSameEmail)
        {
            return Result.Fail("Email already in use");
        }

        var update = Builders<User>.Update
            .Set(u => u.Email, request.Email)
            .Set(u => u.Password, request.Password)
            .Set(u => u.FirstName, request.FirstName)
            .Set(u => u.LastName, request.LastName);

        var result = await _users.UpdateOneAsync(u => u.Id == id, update, cancellationToken: ct);
        if (result.MatchedCount == 0)
        {
            return Result.Fail("Not found");
        }

        return Result.Success();
    }

    public async Task<Result> DeleteUserAsync(ObjectId id, CancellationToken ct)
    {
        var result = await _users.DeleteOneAsync(u => u.Id == id, ct);
        if (result.DeletedCount == 0)
        {
            return Result.Fail("Not found");
        }

        return Result.Success();
    }
}
