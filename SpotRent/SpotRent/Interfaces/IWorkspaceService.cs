using MongoDB.Bson;
using MongoDB.Driver.GeoJsonObjectModel;
using SpotRent.Dto;
using SpotRent.Entities;
using SpotRent.Models;

namespace SpotRent.Interfaces;

public interface IWorkspaceService
{
    Task<Result> CreateWorkspaceAsync(CreateWorkspaceRequest request, CancellationToken ct = default);
    Task<Result<IEnumerable<WorkSpaceDto>>> GetWorkspacesAsync(CancellationToken ct = default);
    Task<Result<WorkSpaceDto>> GetWorkspaceAsync(ObjectId id, CancellationToken ct = default);
    Task<Result> UpdateWorkspaceAsync(ObjectId id, CreateWorkspaceRequest request, CancellationToken ct = default);
    Task<Result> DeleteWorkspaceAsync(ObjectId id, CancellationToken ct = default);

    Task<Result> DeactivateWorkspacesMatchingTextAsync(string textMatch, CancellationToken ct);
    Task<Result> AppendTextToWorkspaceNameAsync(ObjectId workSpaceId, string suffix, CancellationToken ct);

    Task<Result<IEnumerable<WorkSpace>>> FindWorkspacesNearLocationAsync(GeoJson2DCoordinates center, double radiusKm,
        CancellationToken ct);
    Task<Result<int>> AppendTextToMultipleWorkspacesByNamePatternAsync(string namePattern, string suffix,
        CancellationToken ct);
    Task<Result<IEnumerable<WorkSpace>>> FindWorkspacesByNamePatternAsync(string pattern, CancellationToken ct);
}
