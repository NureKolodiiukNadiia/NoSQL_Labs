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

        group.MapGet("", GetUserByIdAsync);
        group.MapPost("/with-id", CreateWithKnowIdAsync);
        group.MapPost("", CreateWithAutoIdAsync);
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
}
