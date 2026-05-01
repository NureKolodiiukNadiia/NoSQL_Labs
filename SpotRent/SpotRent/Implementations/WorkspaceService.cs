using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GeoJsonObjectModel;
using SpotRent.Dto;
using SpotRent.Entities;
using SpotRent.Interfaces;
using SpotRent.Data;
using SpotRent.Models;

namespace SpotRent.Implementations;

public sealed class WorkspaceService : IWorkspaceService
{
    private readonly IMongoCollection<WorkSpace> _workspaces;

    public WorkspaceService(MongoDbContext db)
    {
        _workspaces = db.Workspaces;
    }

    public async Task<Result> CreateWorkspaceAsync(CreateWorkspaceRequest request, CancellationToken ct)
    {
        var workspace = new WorkSpace(
            ObjectId.GenerateNewId(),
            request.Name,
            request.SpaceType,
            request.HourlyRate,
            request.Capacity,
            request.Amenities,
            request.Location,
            request.IsActive);

        await _workspaces.InsertOneAsync(workspace, ct);
        return Result.Success();
    }

    public async Task<Result<IEnumerable<WorkSpaceDto>>> GetWorkspacesAsync(CancellationToken ct)
    {
        var workspaces = await _workspaces.Find(FilterDefinition<WorkSpace>.Empty).ToListAsync(ct);
        return Result.Success(workspaces.Select(w =>
            new WorkSpaceDto(
                w.Id.ToString(),
                w.Name,
                w.SpaceType,
                w.HourlyRate,
                w.Capacity,
                w.Amenities,
                w.Location,
                w.IsActive)));
    }

    public async Task<Result<WorkSpaceDto>> GetWorkspaceAsync(ObjectId id, CancellationToken ct)
    {
        var w = await _workspaces.Find(w => w.Id == id).FirstOrDefaultAsync(ct);
        if (w is null)
        {
            return Result.Fail<WorkSpaceDto>("Workspace not found");
        }

        WorkSpaceDto dto = new WorkSpaceDto(w.Id.ToString(),
            w.Name,
            w.SpaceType,
            w.HourlyRate,
            w.Capacity,
            w.Amenities,
            w.Location,
            w.IsActive);
        return Result.Success(dto);
    }

    public async Task<Result> UpdateWorkspaceAsync(ObjectId id, CreateWorkspaceRequest request, CancellationToken ct)
    {
        var update = Builders<WorkSpace>.Update
            .Set(w => w.Name, request.Name)
            .Set(w => w.SpaceType, request.SpaceType)
            .Set(w => w.HourlyRate, request.HourlyRate)
            .Set(w => w.Capacity, request.Capacity)
            .Set(w => w.Amenities, request.Amenities ?? new List<string>())
            .Set(w => w.Location, request.Location)
            .Set(w => w.IsActive, request.IsActive);

        var result = await _workspaces.UpdateOneAsync(w => w.Id == id, update, cancellationToken: ct);
        if (result.MatchedCount == 0)
        {
            return Result.Fail("Workspace not found");
        }

        return Result.Success();
    }

    public async Task<Result> DeleteWorkspaceAsync(ObjectId id, CancellationToken ct)
    {
        var result = await _workspaces.DeleteOneAsync(w => w.Id == id, ct);
        if (result.DeletedCount == 0)
        {
            return Result.Fail("Workspace not found");
        }

        return Result.Success();
    }

    public async Task<Result> DeactivateWorkspacesMatchingTextAsync(string textMatch, CancellationToken ct)
    {
        try
        {
            var filter = Builders<WorkSpace>.Filter.Regex(
                w => w.Name, new BsonRegularExpression(textMatch, "i"));
            var update = Builders<WorkSpace>.Update.Set(w => w.IsActive, false);

            var result = await _workspaces.UpdateManyAsync(filter, update, new UpdateOptions(), ct);

            if (!result.IsAcknowledged)
            {
                return Result.Fail("Update operation was not acknowledged by MongoDB.");
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to update workspaces: {ex.Message}");
        }
    }

    public async Task<Result> AppendTextToWorkspaceNameAsync(ObjectId workSpaceId, string suffix,
        CancellationToken ct)
    {
        try
        {
            var filter = Builders<WorkSpace>.Filter.Eq(space => space.Id, workSpaceId);
            var concatArray = new BsonArray
            {
                new BsonDocument("$ifNull", new BsonArray { "$Name", "" }),
                suffix
            };
            var setDoc = new BsonDocument("Name", new BsonDocument("$concat", concatArray));
            var updatePipeline = new EmptyPipelineDefinition<WorkSpace>()
                .AppendStage<WorkSpace, WorkSpace, WorkSpace>(new BsonDocument("$set", setDoc));
            var update = Builders<WorkSpace>.Update.Pipeline(updatePipeline);
            var updateResult = await _workspaces.UpdateOneAsync(filter, update, new UpdateOptions(), ct);
            if (!updateResult.IsAcknowledged)
            {
                return Result.Fail("Update operation was not acknowledged by MongoDB.");
            }

            if (updateResult.MatchedCount == 0)
            {
                return Result.Fail("Workspace not found");
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to append text to workspace name: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<WorkSpace>>> FindWorkspacesNearLocationAsync(GeoJson2DCoordinates center,
        double radiusKm, CancellationToken ct)
    {
        try
        {
            var centerPoint = new GeoJsonPoint<GeoJson2DCoordinates>(center);
            var maxDistanceMeters = radiusKm * 1000.0;
            var notNullFilter = Builders<WorkSpace>.Filter.Ne(w => w.Location, null);
            var nearFilter = Builders<WorkSpace>.Filter.Near(w => w.Location, centerPoint, maxDistanceMeters);
            var combinedFilter = Builders<WorkSpace>.Filter.And(notNullFilter, nearFilter);
            var list = await _workspaces.Find(combinedFilter).ToListAsync(ct);

            return Result.Success<IEnumerable<WorkSpace>>(list);
        }
        catch (Exception e)
        {
            return Result.Fail<IEnumerable<WorkSpace>>(e.Message);
        }
    }

    public async Task<Result<int>> AppendTextToMultipleWorkspacesByNamePatternAsync(string namePattern, string suffix,
        CancellationToken ct)
    {
        try
        {
            var filter = Builders<WorkSpace>.Filter.Regex(w => w.Name, new BsonRegularExpression(namePattern, "i"));
            var concatArray = new BsonArray
            {
                new BsonDocument("$ifNull", new BsonArray { "$Name", "" }),
                suffix
            };
            var setDoc = new BsonDocument("Name", new BsonDocument("$concat", concatArray));
            var updatePipeline = new EmptyPipelineDefinition<WorkSpace>()
                .AppendStage<WorkSpace, WorkSpace, WorkSpace>(new BsonDocument("$set", setDoc));
            var update = Builders<WorkSpace>.Update.Pipeline(updatePipeline);
            var result = await _workspaces.UpdateManyAsync(filter, update, new UpdateOptions(), ct);

            if (!result.IsAcknowledged)
            {
                return Result.Fail<int>("Update operation was not acknowledged by MongoDB.");
            }

            return Result.Success((int)result.ModifiedCount);
        }
        catch (Exception ex)
        {
            return Result.Fail<int>($"Failed to update workspaces: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<WorkSpace>>> FindWorkspacesByNamePatternAsync(string pattern,
        CancellationToken ct)
    {
        try
        {
            var filter = Builders<WorkSpace>.Filter.Or(
                Builders<WorkSpace>.Filter.Regex(w => w.Name, new BsonRegularExpression(pattern, "i"))
            );
            var list = await _workspaces.Find(filter).ToListAsync(ct);
            return Result.Success<IEnumerable<WorkSpace>>(list);
        }
        catch (Exception e)
        {
            return Result.Fail<IEnumerable<WorkSpace>>(e.Message);
        }
    }
}
