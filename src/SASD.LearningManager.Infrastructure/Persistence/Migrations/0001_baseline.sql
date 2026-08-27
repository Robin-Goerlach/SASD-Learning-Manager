CREATE TABLE IF NOT EXISTS ActivityLog (
    Id TEXT NOT NULL PRIMARY KEY,
    EntityType TEXT NOT NULL,
    EntityId TEXT NOT NULL,
    ActivityType TEXT NOT NULL,
    OccurredAtUtc TEXT NOT NULL,
    Summary TEXT NOT NULL,
    MetadataJson TEXT NULL
);

CREATE INDEX IF NOT EXISTS IX_ActivityLog_Entity ON ActivityLog (EntityType, EntityId, OccurredAtUtc DESC);
