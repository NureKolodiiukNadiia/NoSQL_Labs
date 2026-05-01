const dbName = 'lab-db';
db = db.getSiblingDB(dbName);
// 30.2305, 50.4125 | 36
db.users.deleteMany({});
db.workspaces.deleteMany({});
db.bookings.deleteMany({});

const users = [
  {
    _id: ObjectId('66f000000000000000000001'),
    Email: 'anna.ivanenko@example.com',
    Password: 'StrongPass1!',
    FirstName: 'Anna',
    LastName: 'Ivanenko'
  },
  {
    _id: ObjectId('66f000000000000000000002'),
    Email: 'oleg.koval@example.com',
    Password: 'StrongPass2!',
    FirstName: 'Oleh'
  },
  {
    _id: ObjectId('66f000000000000000000003'),
    Email: 'maria.shevchenko@example.com',
    Password: 'StrongPass3!'
  },
  {
    _id: ObjectId('66f000000000000000000004'),
    Email: 'taras.bondar@example.com',
    Password: 'StrongPass4!',
    LastName: 'Bondar'
  },
  {
    _id: ObjectId('66f000000000000000000005'),
    Email: 'inna.lebid@example.com',
    Password: 'StrongPass5!',
    FirstName: 'Inna',
    LastName: 'Lebid'
  }
];

const workspaces = [
  {
    _id: ObjectId('66f100000000000000000001'),
    Name: 'Skyline Meeting Room',
    SpaceType: 0,
    HourlyRate: NumberDecimal('450.00'),
    Capacity: 8,
    Amenities: ['Whiteboard', 'TV', 'Coffee'],
    Location: { type: 'Point', coordinates: [30.5234, 50.4501] },
    IsActive: true
  },
  {
    _id: ObjectId('66f100000000000000000002'),
    Name: 'Downtown Hot Desk Cluster',
    SpaceType: 1,
    HourlyRate: NumberDecimal('120.00'),
    Capacity: 18,
    Amenities: ['Wi-Fi', 'Printer'],
    Location: { type: 'Point', coordinates: [30.5148, 50.4482] },
    IsActive: true
  },
  {
    _id: ObjectId('66f100000000000000000003'),
    Name: 'Quiet Private Office',
    SpaceType: 2,
    HourlyRate: NumberDecimal('700.00'),
    Capacity: 4,
    Amenities: ['Lockable door', 'Desk lamp', 'Ergonomic chair'],
    Location: { type: 'Point', coordinates: [30.5051, 50.4547] },
    IsActive: false
  },
  {
    _id: ObjectId('66f100000000000000000004'),
    Name: 'River View Collaboration Room',
    SpaceType: 0,
    HourlyRate: NumberDecimal('500.00'),
    Amenities: ['Projector', 'Flipchart'],
    Location: { type: 'Point', coordinates: [30.5326, 50.4474] },
    IsActive: true
  },
  {
    _id: ObjectId('66f100000000000000000005'),
    Name: 'Flexible Desk Near Window',
    SpaceType: 1,
    HourlyRate: NumberDecimal('90.00'),
    Capacity: 6,
    Amenities: ['Natural light'],
    Location: {"type": "Point", "coordinates": [30.5326, 50.4474]},
    IsActive: true
  }
];

const bookings = [
  {
    _id: ObjectId('66f200000000000000000001'),
    WorkspaceId: workspaces[0]._id,
    UserId: users[0]._id,
    StartTime: new Date('2026-04-10T09:00:00Z'),
    EndTime: new Date('2026-04-10T11:00:00Z'),
    TotalAmount: NumberDecimal('900.00'),
    Status: 3
  },
  {
    _id: ObjectId('66f200000000000000000002'),
    WorkspaceId: workspaces[1]._id,
    UserId: users[1]._id,
    StartTime: new Date('2026-04-11T13:30:00Z'),
    EndTime: new Date('2026-04-11T15:00:00Z'),
    TotalAmount: NumberDecimal('180.00'),
    Status: 1
  },
  {
    _id: ObjectId('66f200000000000000000003'),
    WorkspaceId: workspaces[2]._id,
    UserId: users[2]._id,
    StartTime: new Date('2026-04-12T08:00:00Z'),
    EndTime: new Date('2026-04-12T12:00:00Z'),
    TotalAmount: NumberDecimal('2800.00'),
    Status: 0
  },
  {
    _id: ObjectId('66f200000000000000000004'),
    WorkspaceId: workspaces[3]._id,
    UserId: users[0]._id,
    StartTime: new Date('2026-04-13T10:15:00Z'),
    EndTime: new Date('2026-04-13T12:45:00Z'),
    TotalAmount: NumberDecimal('1250.00'),
    Status: 2
  },
  {
    _id: ObjectId('66f200000000000000000005'),
    WorkspaceId: workspaces[4]._id,
    UserId: users[4]._id,
    StartTime: new Date('2026-04-14T14:00:00Z'),
    EndTime: new Date('2026-04-14T17:30:00Z'),
    TotalAmount: NumberDecimal('315.00'),
    Status: 1
  }
];

db.users.insertMany(users);
db.workspaces.insertMany(workspaces);
db.bookings.insertMany(bookings);

print(`Seeded ${users.length} users, ${workspaces.length} workspaces, and ${bookings.length} bookings in ${dbName}.`);
