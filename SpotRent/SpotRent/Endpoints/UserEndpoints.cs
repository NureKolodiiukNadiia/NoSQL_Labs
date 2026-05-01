using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using SpotRent.Dto;
using SpotRent.Interfaces;

namespace SpotRent.Endpoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/users").WithTags("Users");

        group.MapGet("", GetUsersAsync);
        group.MapGet("{id}", GetUserByIdAsync);
        group.MapPost("/with-id", CreateWithKnowIdAsync);
        group.MapPost("", CreateWithAutoIdAsync);
        group.MapPut("{id}", UpdateUserAsync);
        group.MapDelete("{id}", DeleteUserAsync);
    }

    private static async Task<IResult> GetUsersAsync(IUserService svc, CancellationToken ct)
    {
        var res = await svc.GetUsersAsync(ct);

        return res.IsSuccess switch
        {
            true => Results.Ok(res.Value),
            _ => Results.Problem(
                title: "Problem getting users",
                detail: res.Error,
                statusCode: StatusCodes.Status400BadRequest)
        };
    }

    private static async Task<IResult> GetUserByIdAsync(IUserService svc, string id, CancellationToken ct)
    {
        var isParsed = ObjectId.TryParse(id, out var objectId);
        if (!isParsed)
        {
            return Results.BadRequest("Not parsed");
        }

        var res = await svc.GetUserAsync(objectId, ct);

        return res.IsSuccess switch
        {
            true => Results.Ok(res.Value),
            _ => Results.Problem(
                title: "Problem getting user",
                detail: res.Error,
                statusCode: StatusCodes.Status400BadRequest)
        };
    }

    private static async Task<IResult> CreateWithKnowIdAsync(IUserService svc, [FromBody] CreateUserRequest request,
        CancellationToken ct)
    {
        var res = await svc.CreateUserAsync(request, ct);

        return res.IsSuccess switch
        {
            true => Results.Ok(),
            _ => Results.Problem(
                title: "Problem creating user",
                detail: res.Error,
                statusCode: StatusCodes.Status400BadRequest)
        };
    }

    private static async Task<IResult> CreateWithAutoIdAsync(IUserService svc, [FromBody] CreateUserRequest request,
        CancellationToken ct)
    {
        if (request.Id.HasValue)
        {
            request.Id = null;
        }

        var res = await svc.CreateUserAsync(request, ct);

        return res.IsSuccess switch
        {
            true => Results.Ok(),
            _ => Results.Problem(
                title: "Problem creating user",
                detail: res.Error,
                statusCode: StatusCodes.Status400BadRequest)
        };
    }

    private static async Task<IResult> UpdateUserAsync(IUserService svc, string id, [FromBody] CreateUserRequest request,
        CancellationToken ct)
    {
        var isParsed = ObjectId.TryParse(id, out var objectId);
        if (!isParsed)
        {
            return Results.BadRequest("Not parsed");
        }

        request.Id = objectId;
        var res = await svc.UpdateUserAsync(objectId, request, ct);

        return res.IsSuccess switch
        {
            true => Results.NoContent(),
            _ => Results.Problem(
                title: "Problem updating user",
                detail: res.Error,
                statusCode: StatusCodes.Status400BadRequest)
        };
    }

    private static async Task<IResult> DeleteUserAsync(IUserService svc, string id, CancellationToken ct)
    {
        var isParsed = ObjectId.TryParse(id, out var objectId);
        if (!isParsed)
        {
            return Results.BadRequest("Not parsed");
        }

        var res = await svc.DeleteUserAsync(objectId, ct);

        return res.IsSuccess switch
        {
            true => Results.NoContent(),
            _ => Results.Problem(
                title: "Problem deleting user",
                detail: res.Error,
                statusCode: StatusCodes.Status400BadRequest)
        };
    }
}
