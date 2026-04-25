using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver.GeoJsonObjectModel;
using SpotRent.Dto;
using SpotRent.Interfaces;

namespace SpotRent.Endpoints;

public static class WorkspaceEndpoints
{
    public static void MapWorkspaceEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/workspaces").WithTags("Workspaces");

        group.MapPatch("/deactivate", DeactivateMatchingTextAsync);
        group.MapPatch("/append-name", AppendTextAsync);
        group.MapPatch("/append-name-by-pattern", AppendTextToMultipleAsync);
        group.MapGet("/near", FindWorkspacesNearLocationAsync);
        group.MapGet("/by-pattern", FindByPatternAsync);
    }

    private static async Task<IResult> FindWorkspacesNearLocationAsync(IWorkspaceService svc,
        [FromBody] FindWorkspacesNearLocationRequest request, CancellationToken ct)
    {
        if (!request.IsValid)
        {
            return Results.Problem(
                title: "Request is not valid",
                detail: "Business logic validation did not pass",
                statusCode: StatusCodes.Status400BadRequest);
        }

        var coordinates = new GeoJson2DCoordinates(request.X, request.Y);
        var res = await svc.FindWorkspacesNearLocationAsync(
            coordinates, request.RadiusKm, ct);

        return res.IsSuccess switch
        {
            true => Results.Ok(res.Value),
            _ => Results.BadRequest(res.Error)
        };
    }

    private static async Task<IResult> DeactivateMatchingTextAsync(IWorkspaceService svc, string textMatch,
        CancellationToken ct)
    {
        var res = await svc.DeactivateWorkspacesMatchingTextAsync(textMatch, ct);

        return res.IsSuccess switch
        {
            true => Results.NoContent(),
            _ => Results.Problem(
                title: "Failed to update workspaces",
                detail: res.Error,
                statusCode: StatusCodes.Status500InternalServerError)
        };
    }

    private static async Task<IResult> AppendTextAsync(IWorkspaceService svc, string id, string suffix,
        CancellationToken ct)
    {
        if (!ObjectId.TryParse(id, out var objectId))
        {
            return Results.BadRequest("Invalid Workspace ID format.");
        }

        var result = await svc.AppendTextToWorkspaceNameAsync(objectId, suffix, ct);

        return result.IsSuccess switch
        {
            true => Results.NoContent(),
            _ => Results.Problem(
                title: "Failed to append text to workspace",
                detail: result.Error,
                statusCode: StatusCodes.Status400BadRequest)
        };
    }

    private static async Task<IResult> AppendTextToMultipleAsync(IWorkspaceService svc, string pattern, string suffix,
        CancellationToken ct)
    {
        var result = await svc.AppendTextToMultipleWorkspacesByNamePatternAsync(pattern, suffix, ct);

        return result.IsSuccess switch
        {
            true => Results.Ok(new { ModifiedCount = result.Value }),
            _ => Results.Problem(
                title: "Failed to update workspaces",
                detail: result.Error,
                statusCode: StatusCodes.Status500InternalServerError)
        };
    }

    private static async Task<IResult> FindByPatternAsync(IWorkspaceService svc, string pattern, CancellationToken ct)
    {
        var result = await svc.FindWorkspacesByNamePatternAsync(pattern, ct);

        return result.IsSuccess switch
        {
            true => Results.Ok(result.Value),
            _ => Results.Problem(
                title: "Failed to find workspaces",
                detail: result.Error,
                statusCode: StatusCodes.Status500InternalServerError)
        };
    }
}
