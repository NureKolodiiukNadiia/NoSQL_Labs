# NosqlFrontend

This project was generated using [Angular CLI](https://github.com/angular/angular-cli) version 21.2.7.

## Development server

To start a local development server, run:

```bash
ng serve
```

Once the server is running, open your browser and navigate to `http://localhost:4200/`. The application will automatically reload whenever you modify any of the source files.

## Code scaffolding

Angular CLI includes powerful code scaffolding tools. To generate a new component, run:

```bash
ng generate component component-name
```

For a complete list of available schematics (such as `components`, `directives`, or `pipes`), run:

```bash
ng generate --help
```

## Building

To build the project run:

```bash
ng build
```

This will compile your project and store the build artifacts in the `dist/` directory. By default, the production build optimizes your application for performance and speed.

## Running unit tests

To execute unit tests with the [Vitest](https://vitest.dev/) test runner, use the following command:

```bash
ng test
```

## Running end-to-end tests

For end-to-end (e2e) testing, run:

```bash
ng e2e
```

Angular CLI does not come with an end-to-end testing framework by default. You can choose one that suits your needs.

## Additional Resources

For more information on using the Angular CLI, including detailed command references, visit the [Angular CLI Overview and Command Reference](https://angular.dev/tools/cli) page.

Set up this minimal Angular app structure with one standalone page containing three sections: Users, Workspaces, Bookings. Keep it as a single-screen admin/demo UI with tabs or anchor buttons only if needed. No routing is required.
Create API service wrappers that map exactly to the existing backend operations:
UserService: createUser, getUserById
WorkspaceService: findByNamePattern, deactivateMatchingText, appendTextToWorkspaceName, appendTextToMultipleByNamePattern, findNearLocation
BookingService: createBooking, getUserBookingCounts
internal sealed class BookingService : IBookingService { private readonly IMongoCollection<Booking> _bookings; private readonly IMongoCollection<User> _users; public BookingService(MongoDbContext db) { _bookings = db.Bookings; _users = db.Users; } public async Task<Result> CreateBookingAsync(ObjectId userId, ObjectId workspaceId, CancellationToken ct) { ct.ThrowIfCancellationRequested(); var booking = new Booking(ObjectId.GenerateNewId(), workspaceId, userId, DateTime.UtcNow, DateTime.UtcNow.AddHours(2), 50.0m, BookingStatus.Confirmed); await _bookings.InsertOneAsync(booking, ct); return Result.Success(); } public async Task<Result<IEnumerable<BookingCountDto>>> GetUserBookingCountsAsync(CancellationToken ct) { ct.ThrowIfCancellationRequested(); var aggregate = await _users.Aggregate() .AppendStage<BsonDocument>(new BsonDocument("$lookup", new BsonDocument { { "from", "bookings" }, { "let", new BsonDocument("userId", "$Id") }, { "pipeline", new BsonArray { new BsonDocument("$match", new BsonDocument("$expr", new BsonDocument("$eq", new BsonArray { "$UserId", "$$userId" }) ) ), new BsonDocument("$count", "count") } }, { "as", "booking_meta" } } )) .Project(new BsonDocument { { "Email", 1 }, { "bookings_count", new BsonDocument("$ifNull", new BsonArray { new BsonDocument("$arrayElemAt", new BsonArray { "$booking_meta.count", 0 }), 0 }) } }) .ToListAsync(ct); var dtos = aggregate.Select(x => new BookingCountDto( x.GetValue("Email", string.Empty).AsString, x.GetValue("bookings_count", 0).AsInt32 )); return Result.Success(dtos); } } using MongoDB.Bson; using MongoDB.Driver; using SpotRent.Entities; using SpotRent.Interfaces; using SpotRent.Data; using SpotRent.Dto; using SpotRent.Models; namespace SpotRent.Implementations; public sealed class UserService : IUserService { private readonly IMongoCollection<User> _users; public UserService(MongoDbContext db) { _users = db.Users; } public async Task<Result> CreateUserAsync(CreateUserRequest request, CancellationToken ct) { if (await _users .Find(u => u.Id == (request.Id ?? ObjectId.Empty) || u.Email == request.Email) .AnyAsync(ct)) { return Result.Fail("User exists"); } var user = new User( request.Id ?? ObjectId.GenerateNewId(), request.Email, request.Password, request.FirstName, request.LastName); await _users.InsertOneAsync(user, ct); return Result.Success(); } public async Task<Result<UserDto>> GetUserAsync(ObjectId id, CancellationToken ct) { var userDto = await _users .Find(u => u.Id == id) .Project(u => new UserDto(u.Id, u.Email, u.FirstName, u.LastName)) .FirstOrDefaultAsync(ct); if (userDto is null) { return Result.Fail<UserDto>("Not found"); } return Result.Success(userDto); } } using MongoDB.Bson; using MongoDB.Driver; using MongoDB.Driver.GeoJsonObjectModel; using SpotRent.Entities; using SpotRent.Interfaces; using SpotRent.Data; using SpotRent.Models; namespace SpotRent.Implementations; public sealed class WorkspaceService : IWorkspaceService { private readonly IMongoCollection<WorkSpace> _workspaces; public WorkspaceService(MongoDbContext db) { _workspaces = db.Workspaces; } public async Task<Result> DeactivateWorkspacesMatchingTextAsync(string textMatch, CancellationToken ct) { try { var filter = Builders<WorkSpace>.Filter.Regex( w => w.Name, new BsonRegularExpression(textMatch, "i")); var update = Builders<WorkSpace>.Update.Set(w => w.IsActive, false); var result = await _workspaces.UpdateManyAsync(filter, update, new UpdateOptions(), ct); if (!result.IsAcknowledged) { return Result.Fail("Update operation was not acknowledged by MongoDB."); } return Result.Success(); } catch (Exception ex) { return Result.Fail($"Failed to update workspaces: {ex.Message}"); } } public async Task<Result> AppendTextToWorkspaceNameAsync(ObjectId workSpaceId, string suffix, CancellationToken ct) { try { var filter = Builders<WorkSpace>.Filter.Eq(space => space.Id, workSpaceId); var concatArray = new BsonArray { new BsonDocument("$ifNull", new BsonArray { "$Name", "" }), suffix }; var setDoc = new BsonDocument("Name", new BsonDocument("$concat", concatArray)); var updatePipeline = new EmptyPipelineDefinition<WorkSpace>() .AppendStage<WorkSpace, WorkSpace, WorkSpace>(new BsonDocument("$set", setDoc)); var update = Builders<WorkSpace>.Update.Pipeline(updatePipeline); var updateResult = await _workspaces.UpdateOneAsync(filter, update, new UpdateOptions(), ct); if (!updateResult.IsAcknowledged) { return Result.Fail("Update operation was not acknowledged by MongoDB."); } if (updateResult.MatchedCount == 0) { return Result.Fail("Workspace not found"); } return Result.Success(); } catch (Exception ex) { return Result.Fail($"Failed to append text to workspace name: {ex.Message}"); } } public async Task<Result<IEnumerable<WorkSpace>>> FindWorkspacesNearLocationAsync(GeoJson2DCoordinates center, double radiusKm, CancellationToken ct) { try { var centerPoint = new GeoJsonPoint<GeoJson2DCoordinates>(center); var maxDistanceMeters = radiusKm * 1000.0; var notNullFilter = Builders<WorkSpace>.Filter.Ne(w => w.Location, null); var nearFilter = Builders<WorkSpace>.Filter.Near(w => w.Location, centerPoint, maxDistanceMeters); var combinedFilter = Builders<WorkSpace>.Filter.And(notNullFilter, nearFilter); var list = await _workspaces.Find(combinedFilter).ToListAsync(ct); return Result.Success<IEnumerable<WorkSpace>>(list); } catch (Exception e) { return Result.Fail<IEnumerable<WorkSpace>>(e.Message); } } public async Task<Result<int>> AppendTextToMultipleWorkspacesByNamePatternAsync(string namePattern, string suffix, CancellationToken ct) { try { var filter = Builders<WorkSpace>.Filter.Regex(w => w.Name, new BsonRegularExpression(namePattern, "i")); var concatArray = new BsonArray { new BsonDocument("$ifNull", new BsonArray { "$Name", "" }), suffix }; var setDoc = new BsonDocument("Name", new BsonDocument("$concat", concatArray)); var updatePipeline = new EmptyPipelineDefinition<WorkSpace>() .AppendStage<WorkSpace, WorkSpace, WorkSpace>(new BsonDocument("$set", setDoc)); var update = Builders<WorkSpace>.Update.Pipeline(updatePipeline); var result = await _workspaces.UpdateManyAsync(filter, update, new UpdateOptions(), ct); if (!result.IsAcknowledged) { return Result.Fail<int>("Update operation was not acknowledged by MongoDB."); } return Result.Success<int>((int)result.ModifiedCount); } catch (Exception ex) { return Result.Fail<int>($"Failed to update workspaces: {ex.Message}"); } } public async Task<Result<IEnumerable<WorkSpace>>> FindWorkspacesByNamePatternAsync(string pattern, CancellationToken ct) { try { var filter = Builders<WorkSpace>.Filter.Or( Builders<WorkSpace>.Filter.Regex(w => w.Name, new BsonRegularExpression(pattern, "i")) ); var list = await _workspaces.Find(filter).ToListAsync(ct); return Result.Success<IEnumerable<WorkSpace>>(list); } catch (Exception e) { return Result.Fail<IEnumerable<WorkSpace>>(e.Message); } } }
