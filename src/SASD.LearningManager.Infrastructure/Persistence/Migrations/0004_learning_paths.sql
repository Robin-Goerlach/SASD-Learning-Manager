CREATE TABLE LearningPaths (
    Id TEXT NOT NULL PRIMARY KEY,
    Title TEXT NOT NULL,
    Description TEXT NULL,
    Status TEXT NOT NULL,
    Priority TEXT NOT NULL,
    PlannedStartDate TEXT NULL,
    TargetDate TEXT NULL,
    NextActionText TEXT NULL,
    NextActionDueDate TEXT NULL,
    CreatedAtUtc TEXT NOT NULL,
    UpdatedAtUtc TEXT NOT NULL,
    StartedAtUtc TEXT NULL,
    CompletedAtUtc TEXT NULL,
    ArchivedAtUtc TEXT NULL
);

CREATE TABLE GoalLearningPaths (
    GoalId TEXT NOT NULL,
    LearningPathId TEXT NOT NULL,
    PRIMARY KEY (GoalId, LearningPathId),
    FOREIGN KEY (GoalId) REFERENCES Goals(Id) ON DELETE RESTRICT,
    FOREIGN KEY (LearningPathId) REFERENCES LearningPaths(Id) ON DELETE CASCADE
);

CREATE TABLE LearningPathNodes (
    Id TEXT NOT NULL PRIMARY KEY,
    LearningPathId TEXT NOT NULL,
    ParentNodeId TEXT NULL,
    Title TEXT NOT NULL,
    Description TEXT NULL,
    NodeType TEXT NOT NULL,
    SortOrder INTEGER NOT NULL CHECK (SortOrder >= 0),
    IsRequired INTEGER NOT NULL CHECK (IsRequired IN (0, 1)),
    Status TEXT NOT NULL,
    CreatedAtUtc TEXT NOT NULL,
    UpdatedAtUtc TEXT NOT NULL,
    ArchivedAtUtc TEXT NULL,
    FOREIGN KEY (LearningPathId) REFERENCES LearningPaths(Id) ON DELETE CASCADE,
    FOREIGN KEY (ParentNodeId) REFERENCES LearningPathNodes(Id) ON DELETE RESTRICT
);

CREATE TABLE LearningPathNodeSkills (
    LearningPathNodeId TEXT NOT NULL,
    SkillId TEXT NOT NULL,
    PRIMARY KEY (LearningPathNodeId, SkillId),
    FOREIGN KEY (LearningPathNodeId) REFERENCES LearningPathNodes(Id) ON DELETE CASCADE,
    FOREIGN KEY (SkillId) REFERENCES Skills(Id) ON DELETE RESTRICT
);

CREATE TABLE LearningPathNodeResources (
    LearningPathNodeId TEXT NOT NULL,
    ResourceId TEXT NOT NULL,
    PRIMARY KEY (LearningPathNodeId, ResourceId),
    FOREIGN KEY (LearningPathNodeId) REFERENCES LearningPathNodes(Id) ON DELETE CASCADE,
    FOREIGN KEY (ResourceId) REFERENCES Resources(Id) ON DELETE RESTRICT
);

CREATE TABLE LearningPathNodeRelations (
    Id TEXT NOT NULL PRIMARY KEY,
    SourceNodeId TEXT NOT NULL,
    TargetNodeId TEXT NOT NULL,
    RelationType TEXT NOT NULL,
    Note TEXT NULL,
    CreatedAtUtc TEXT NOT NULL,
    CHECK (SourceNodeId <> TargetNodeId),
    UNIQUE (SourceNodeId, TargetNodeId, RelationType),
    FOREIGN KEY (SourceNodeId) REFERENCES LearningPathNodes(Id) ON DELETE CASCADE,
    FOREIGN KEY (TargetNodeId) REFERENCES LearningPathNodes(Id) ON DELETE CASCADE
);

CREATE INDEX IX_LearningPaths_Status ON LearningPaths (Status);
CREATE INDEX IX_LearningPaths_TargetDate ON LearningPaths (TargetDate);
CREATE INDEX IX_LearningPaths_UpdatedAt ON LearningPaths (UpdatedAtUtc DESC);
CREATE INDEX IX_GoalLearningPaths_Path ON GoalLearningPaths (LearningPathId, GoalId);
CREATE INDEX IX_LearningPathNodes_PathParentSort ON LearningPathNodes (LearningPathId, ParentNodeId, SortOrder);
CREATE INDEX IX_LearningPathNodes_Status ON LearningPathNodes (LearningPathId, Status);
CREATE INDEX IX_LearningPathNodeSkills_Skill ON LearningPathNodeSkills (SkillId, LearningPathNodeId);
CREATE INDEX IX_LearningPathNodeResources_Resource ON LearningPathNodeResources (ResourceId, LearningPathNodeId);
CREATE INDEX IX_LearningPathNodeRelations_Target ON LearningPathNodeRelations (TargetNodeId, SourceNodeId);
