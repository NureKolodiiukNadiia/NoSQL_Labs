using Microsoft.Extensions.Options;
using MongoDB.Driver;
using SpotRent.Entities;

namespace SpotRent.Data;

public class MongoDbContext
{
    private readonly IMongoDatabase _db;

    public MongoDbContext(IOptions<MongoOptions> mongoOptions)
    {
        var opts = mongoOptions.Value;
        var client = new MongoClient(opts.ConnectionString);
        _db = client.GetDatabase(opts.DbName);
        Users = _db.GetCollection<User>("users");
        Workspaces = _db.GetCollection<WorkSpace>("workspaces");
        Bookings = _db.GetCollection<Booking>("bookings");
    }

    public IMongoCollection<User> Users { get; }
    public IMongoCollection<WorkSpace> Workspaces { get; }
    public IMongoCollection<Booking> Bookings { get; }
}
