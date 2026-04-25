using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GeoJsonObjectModel;
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

            return Result.Success<int>((int)result.ModifiedCount);
        }
        catch (Exception ex)
        {
            return Result.Fail<int>($"Failed to update workspaces: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<WorkSpace>>> FindWorkspacesByNamePatternAsync(string pattern, CancellationToken ct)
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
