const databaseName = 'MusicPlay';
const bucketName = 'audio-files';

// Switch to (or create) the target database
const db = db.getSiblingDB(databaseName);

// Users collection with case-insensitive uniqueness on username/email
if (!db.getCollectionNames().includes('users')) {
  db.createCollection('users', {
    collation: { locale: 'en', strength: 2 },
  });
}

db.users.createIndex({ username: 1 }, { unique: true, collation: { locale: 'en', strength: 2 } });
db.users.createIndex({ email: 1 }, { unique: true, collation: { locale: 'en', strength: 2 } });

db.users.createIndex({ roles: 1 });
db.users.createIndex({ createdAt: -1 });

// Tracks collection storing metadata for uploaded audio
if (!db.getCollectionNames().includes('tracks')) {
  db.createCollection('tracks');
}

db.tracks.createIndex({ ownerId: 1, createdAt: -1 });
db.tracks.createIndex({ featured: 1, createdAt: -1 });
db.tracks.createIndex({ slug: 1 }, { unique: true, sparse: true });
db.tracks.createIndex({ tags: 1 });
db.tracks.createIndex({ isApproved: 1, createdAt: -1 });
db.tracks.createIndex({ playCount: -1 });

db.tracks.createIndex({ title: 'text', description: 'text', artistName: 'text' }, {
  weights: { title: 5, artistName: 4, description: 2 },
  default_language: 'none',
  name: 'TracksTextSearch',
});

// Playlists collection powering curated lists and recommendations
if (!db.getCollectionNames().includes('playlists')) {
  db.createCollection('playlists');
}

db.playlists.createIndex({ ownerId: 1, createdAt: -1 });
db.playlists.createIndex({ slug: 1 }, { unique: true, sparse: true });

db.playlists.createIndex({ title: 'text', description: 'text' }, {
  weights: { title: 4, description: 2 },
  default_language: 'none',
  name: 'PlaylistsTextSearch',
});

// Listening history collection powering ML-based recommendations
if (!db.getCollectionNames().includes('listeningHistory')) {
  db.createCollection('listeningHistory');
}

db.listeningHistory.createIndex({ userId: 1, playedAt: -1 });
db.listeningHistory.createIndex({ trackId: 1, playedAt: -1 });

// GridFS bucket for audio blobs (audio-files.files & audio-files.chunks)
const filesCollection = `${bucketName}.files`;
const chunksCollection = `${bucketName}.chunks`;

if (!db.getCollectionNames().includes(filesCollection)) {
  db.createCollection(filesCollection);
}
if (!db.getCollectionNames().includes(chunksCollection)) {
  db.createCollection(chunksCollection);
}

db.getCollection(filesCollection).createIndex({ filename: 1, uploadDate: -1 });
db.getCollection(chunksCollection).createIndex({ files_id: 1, n: 1 }, { unique: true });

print(`MongoDB database \"${databaseName}\" is ready with collections and indexes.`);
