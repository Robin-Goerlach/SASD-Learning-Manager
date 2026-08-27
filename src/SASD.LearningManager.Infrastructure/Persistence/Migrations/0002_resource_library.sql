CREATE TABLE Providers (
    Id TEXT NOT NULL PRIMARY KEY,
    Name TEXT NOT NULL COLLATE NOCASE UNIQUE,
    WebsiteUrl TEXT NULL,
    Description TEXT NULL,
    ProviderType TEXT NOT NULL,
    Status TEXT NOT NULL,
    CreatedAtUtc TEXT NOT NULL,
    UpdatedAtUtc TEXT NOT NULL,
    ArchivedAtUtc TEXT NULL
);

CREATE TABLE Resources (
    Id TEXT NOT NULL PRIMARY KEY,
    Title TEXT NOT NULL,
    ResourceType TEXT NOT NULL,
    ProviderId TEXT NULL,
    Url TEXT NULL,
    NormalizedUrl TEXT NULL,
    LocalPath TEXT NULL,
    Description TEXT NULL,
    WhySaved TEXT NULL,
    Creator TEXT NULL,
    LanguageCode TEXT NULL,
    VersionText TEXT NULL,
    EstimatedMinutes INTEGER NULL CHECK (EstimatedMinutes IS NULL OR EstimatedMinutes >= 0),
    Difficulty TEXT NOT NULL,
    Priority TEXT NOT NULL,
    Status TEXT NOT NULL,
    ProgressPercent INTEGER NULL CHECK (ProgressPercent IS NULL OR (ProgressPercent >= 0 AND ProgressPercent <= 100)),
    StartedAtUtc TEXT NULL,
    CompletedAtUtc TEXT NULL,
    CreatedAtUtc TEXT NOT NULL,
    UpdatedAtUtc TEXT NOT NULL,
    ArchivedAtUtc TEXT NULL,
    FOREIGN KEY (ProviderId) REFERENCES Providers(Id) ON DELETE RESTRICT
);

CREATE INDEX IX_Resources_NormalizedUrl_NotNull
    ON Resources (NormalizedUrl)
    WHERE NormalizedUrl IS NOT NULL;
CREATE INDEX IX_Resources_Status ON Resources (Status);
CREATE INDEX IX_Resources_ProviderId ON Resources (ProviderId);
CREATE INDEX IX_Resources_Type ON Resources (ResourceType);
CREATE INDEX IX_Resources_Priority ON Resources (Priority);
CREATE INDEX IX_Resources_UpdatedAt ON Resources (UpdatedAtUtc DESC);

CREATE TABLE Tags (
    Id TEXT NOT NULL PRIMARY KEY,
    Name TEXT NOT NULL COLLATE NOCASE UNIQUE,
    CreatedAtUtc TEXT NOT NULL
);

CREATE TABLE ResourceTags (
    ResourceId TEXT NOT NULL,
    TagId TEXT NOT NULL,
    PRIMARY KEY (ResourceId, TagId),
    FOREIGN KEY (ResourceId) REFERENCES Resources(Id) ON DELETE CASCADE,
    FOREIGN KEY (TagId) REFERENCES Tags(Id) ON DELETE CASCADE
);

CREATE INDEX IX_ResourceTags_TagId ON ResourceTags (TagId, ResourceId);

-- Seed common providers as editable data, not as hard-coded provider integrations.
INSERT INTO Providers (Id, Name, WebsiteUrl, Description, ProviderType, Status, CreatedAtUtc, UpdatedAtUtc)
VALUES
('11111111-1111-4111-8111-111111111111', 'O''Reilly', 'https://www.oreilly.com/', 'O''Reilly Learning and publishing.', 'LearningPlatform', 'Active', strftime('%Y-%m-%dT%H:%M:%fZ','now'), strftime('%Y-%m-%dT%H:%M:%fZ','now')),
('22222222-2222-4222-8222-222222222222', 'LinkedIn Learning', 'https://www.linkedin.com/learning/', 'LinkedIn Learning video courses.', 'LearningPlatform', 'Active', strftime('%Y-%m-%dT%H:%M:%fZ','now'), strftime('%Y-%m-%dT%H:%M:%fZ','now')),
('33333333-3333-4333-8333-333333333333', 'YouTube', 'https://www.youtube.com/', 'Video platform and learning source.', 'LearningPlatform', 'Active', strftime('%Y-%m-%dT%H:%M:%fZ','now'), strftime('%Y-%m-%dT%H:%M:%fZ','now')),
('44444444-4444-4444-8444-444444444444', 'Udemy', 'https://www.udemy.com/', 'Online course marketplace.', 'LearningPlatform', 'Active', strftime('%Y-%m-%dT%H:%M:%fZ','now'), strftime('%Y-%m-%dT%H:%M:%fZ','now')),
('55555555-5555-4555-8555-555555555555', 'Microsoft Learn', 'https://learn.microsoft.com/', 'Microsoft documentation and learning platform.', 'Vendor', 'Active', strftime('%Y-%m-%dT%H:%M:%fZ','now'), strftime('%Y-%m-%dT%H:%M:%fZ','now')),
('66666666-6666-4666-8666-666666666666', 'Red Hat', 'https://www.redhat.com/', 'Red Hat documentation and training.', 'Vendor', 'Active', strftime('%Y-%m-%dT%H:%M:%fZ','now'), strftime('%Y-%m-%dT%H:%M:%fZ','now')),
('77777777-7777-4777-8777-777777777777', 'Eigene Quelle', NULL, 'Eigene Labs, Projekte und lokale Lernmaterialien.', 'Personal', 'Active', strftime('%Y-%m-%dT%H:%M:%fZ','now'), strftime('%Y-%m-%dT%H:%M:%fZ','now'));
