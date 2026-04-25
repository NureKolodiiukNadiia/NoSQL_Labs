using MongoDB.Bson;
using SpotRent.Dto;
using SpotRent.Models;

namespace SpotRent.Interfaces;

public interface IUserService
{
    Task<Result> CreateUserAsync(CreateUserRequest request, CancellationToken ct = default);
    Task<Result<UserDto>> GetUserAsync(ObjectId id, CancellationToken ct = default);
}
