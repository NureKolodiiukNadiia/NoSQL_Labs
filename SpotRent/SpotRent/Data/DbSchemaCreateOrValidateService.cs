using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using SpotRent.Entities;

namespace SpotRent.Data;

public class DbSchemaCreateOrValidateService
{
    private readonly IMongoDatabase _db;

    public DbSchemaCreateOrValidateService(IOptions<MongoOptions> options)
    {
        _db = new MongoClient(options.Value.ConnectionString).GetDatabase(options.Value.DbName);
    }

    public async Task InitializeAsync()
    {
        try
        {
            await CreateUsersCollectionAsync();
            await CreateBookingsCollectionAsync();
            await CreateWorkspacesCollectionAsync();
        }
        catch (Exception e)
        {
            Environment.FailFast($"Db is not initialised properly: {e.Message}");
        }
    }

    private async Task CreateBookingsCollectionAsync()
    {
        const string name = "bookings";
        if (await CollectionExists(name)) return;

        var options = new CreateCollectionOptions<Booking>
        {
            Validator = new BsonDocument
            {
                {
                    "$jsonSchema", new BsonDocument
                    {
                        { "bsonType", "object" },
                        {
                            "required",
                            new BsonArray
                                { "_id", "WorkspaceId", "UserId", "StartTime", "EndTime", "TotalAmount", "Status" }
                        },
                        {
                            "properties", new BsonDocument
                            {
                                { "_id", new BsonDocument { { "bsonType", "objectId" } } },
                                { "WorkspaceId", new BsonDocument { { "bsonType", "objectId" } } },
                                { "UserId", new BsonDocument { { "bsonType", "objectId" } } },
                                { "StartTime", new BsonDocument { { "bsonType", "date" } } },
                                { "EndTime", new BsonDocument { { "bsonType", "date" } } },
                                { "TotalAmount", new BsonDocument { { "bsonType", "decimal" } } },
                                { "Status", new BsonDocument { { "bsonType", "int" } } }
                            }
                        }
                    }
                }
            }
        };

        await _db.CreateCollectionAsync(name, options);
        var collection = _db.GetCollection<Booking>(name);

        await collection.Indexes.CreateOneAsync(Builders<Booking>.IndexKeys.Ascending(b => b.UserId));
        await collection.Indexes.CreateOneAsync(Builders<Booking>.IndexKeys.Ascending(b => b.WorkspaceId));
    }

    private async Task CreateWorkspacesCollectionAsync()
    {
        const string name = "workspaces";
        if (await CollectionExists(name))
        {
            var collection1 =  _db.GetCollection<WorkSpace>(name);
            await collection1.Indexes.CreateOneAsync(Builders<WorkSpace>.IndexKeys.Geo2DSphere(space => space.Location));

            return;
        }

        var options = new CreateCollectionOptions<WorkSpace>
        {
            Validator = new BsonDocument
            {
                {
                    "$jsonSchema", new BsonDocument
                    {
                        { "bsonType", "object" },
                        { "required", new BsonArray { "_id", "Name", "SpaceType", "HourlyRate" } },
                        {
                            "properties", new BsonDocument
                            {
                                { "_id", new BsonDocument { { "bsonType", "objectId" } } },
                                { "Name", new BsonDocument { { "bsonType", "string" } } },
                                { "SpaceType", new BsonDocument { { "bsonType", "int" } } },
                                { "HourlyRate", new BsonDocument { { "bsonType", "decimal" } } },
                                { "Capacity",
                                    new BsonDocument { { "bsonType", new BsonArray { "int", "null" } } } },
                                {
                                    "Amenities",
                                    new BsonDocument
                                    {
                                        { "bsonType", "array, null" },
                                        { "items", new BsonDocument { { "bsonType", "string" } } }
                                    }
                                },
                                { "Location",
                                    new BsonDocument { { "bsonType", new BsonArray { "object", "null" } } } },
                                { "IsActive",
                                    new BsonDocument { { "bsonType", new BsonArray { "bool", "null" } } } }
                            }
                        }
                    }
                }
            }
        };
        await _db.CreateCollectionAsync(name, options);
        var collection = _db.GetCollection<WorkSpace>(name);

        await collection.Indexes.CreateOneAsync(Builders<WorkSpace>.IndexKeys.Text(space => space.Name));
        await collection.Indexes.CreateOneAsync(Builders<WorkSpace>.IndexKeys.Ascending(space => space.Amenities));
        await collection.Indexes.CreateOneAsync(Builders<WorkSpace>.IndexKeys.Geo2DSphere(space => space.Location));
    }

    private async Task CreateUsersCollectionAsync()
    {
        const string name = "users";
        if (await CollectionExists(name)) return;

        var options = new CreateCollectionOptions<User>
        {
            Validator = new BsonDocument
            {
                {
                    "$jsonSchema", new BsonDocument
                    {
                        { "bsonType", "object" },
                        { "required", new BsonArray { "_id", "Email", "Password" } },
                        {
                            "properties", new BsonDocument
                            {
                                { "_id",
                                    new BsonDocument { { "bsonType", "objectId" } } },
                                { "Email",
                                    new BsonDocument { { "bsonType", "string" }, { "pattern", @"^.+@.+$" } } },
                                { "Password",
                                    new BsonDocument { { "bsonType", "string" } } },
                                { "FirstName",
                                    new BsonDocument { { "bsonType", new BsonArray { "string", "null" } } } },
                                { "LastName",
                                    new BsonDocument { { "bsonType", new BsonArray { "string", "null" } } } }
                            }
                        }
                    }
                }
            }
        };
        await _db.CreateCollectionAsync(name, options);
        var collection = _db.GetCollection<User>(name);

        var emailIndexKeys = Builders<User>.IndexKeys.Ascending(user => user.Email);
        var emailIndexOpts = new CreateIndexOptions { Unique = true };
        await collection.Indexes.CreateOneAsync(new CreateIndexModel<User>(emailIndexKeys, emailIndexOpts));
    }

    private async Task<bool> CollectionExists(string collectionName)
    {
        var filter = new BsonDocument("name", collectionName);
        var collections = await _db.ListCollectionsAsync(
            new ListCollectionsOptions() { Filter = filter });

        return await collections.AnyAsync();
    }
}
