CREATE TABLE CompetencyAreas (
    Id TEXT NOT NULL PRIMARY KEY,
    Name TEXT NOT NULL COLLATE NOCASE UNIQUE,
    Description TEXT NULL,
    Status TEXT NOT NULL,
    CreatedAtUtc TEXT NOT NULL,
    UpdatedAtUtc TEXT NOT NULL,
    ArchivedAtUtc TEXT NULL
);

CREATE TABLE Topics (
    Id TEXT NOT NULL PRIMARY KEY,
    Name TEXT NOT NULL COLLATE NOCASE UNIQUE,
    Description TEXT NULL,
    Status TEXT NOT NULL,
    CreatedAtUtc TEXT NOT NULL,
    UpdatedAtUtc TEXT NOT NULL,
    ArchivedAtUtc TEXT NULL
);

CREATE TABLE CompetencyAreaTopics (
    CompetencyAreaId TEXT NOT NULL,
    TopicId TEXT NOT NULL,
    PRIMARY KEY (CompetencyAreaId, TopicId),
    FOREIGN KEY (CompetencyAreaId) REFERENCES CompetencyAreas(Id) ON DELETE CASCADE,
    FOREIGN KEY (TopicId) REFERENCES Topics(Id) ON DELETE CASCADE
);

CREATE TABLE Skills (
    Id TEXT NOT NULL PRIMARY KEY,
    Name TEXT NOT NULL COLLATE NOCASE UNIQUE,
    Description TEXT NULL,
    CurrentLevel INTEGER NULL CHECK (CurrentLevel IS NULL OR (CurrentLevel >= 1 AND CurrentLevel <= 5)),
    TargetLevel INTEGER NULL CHECK (TargetLevel IS NULL OR (TargetLevel >= 1 AND TargetLevel <= 5)),
    Status TEXT NOT NULL,
    CreatedAtUtc TEXT NOT NULL,
    UpdatedAtUtc TEXT NOT NULL,
    ArchivedAtUtc TEXT NULL
);

CREATE TABLE SkillAssessments (
    Id TEXT NOT NULL PRIMARY KEY,
    SkillId TEXT NOT NULL,
    Level INTEGER NOT NULL CHECK (Level >= 1 AND Level <= 5),
    AssessmentType TEXT NOT NULL,
    Reason TEXT NULL,
    AssessedAtUtc TEXT NOT NULL,
    CreatedAtUtc TEXT NOT NULL,
    FOREIGN KEY (SkillId) REFERENCES Skills(Id) ON DELETE CASCADE
);

CREATE TABLE CompetencyAreaSkills (
    CompetencyAreaId TEXT NOT NULL,
    SkillId TEXT NOT NULL,
    PRIMARY KEY (CompetencyAreaId, SkillId),
    FOREIGN KEY (CompetencyAreaId) REFERENCES CompetencyAreas(Id) ON DELETE CASCADE,
    FOREIGN KEY (SkillId) REFERENCES Skills(Id) ON DELETE CASCADE
);

CREATE TABLE TopicSkills (
    TopicId TEXT NOT NULL,
    SkillId TEXT NOT NULL,
    PRIMARY KEY (TopicId, SkillId),
    FOREIGN KEY (TopicId) REFERENCES Topics(Id) ON DELETE CASCADE,
    FOREIGN KEY (SkillId) REFERENCES Skills(Id) ON DELETE CASCADE
);

CREATE TABLE Goals (
    Id TEXT NOT NULL PRIMARY KEY,
    Title TEXT NOT NULL,
    Description TEXT NULL,
    GoalType TEXT NOT NULL,
    Motivation TEXT NULL,
    Priority TEXT NOT NULL,
    Status TEXT NOT NULL,
    TargetDate TEXT NULL,
    NextActionText TEXT NULL,
    NextActionDueDate TEXT NULL,
    CreatedAtUtc TEXT NOT NULL,
    UpdatedAtUtc TEXT NOT NULL,
    AchievedAtUtc TEXT NULL,
    ArchivedAtUtc TEXT NULL
);

CREATE TABLE GoalSkills (
    GoalId TEXT NOT NULL,
    SkillId TEXT NOT NULL,
    PRIMARY KEY (GoalId, SkillId),
    FOREIGN KEY (GoalId) REFERENCES Goals(Id) ON DELETE CASCADE,
    FOREIGN KEY (SkillId) REFERENCES Skills(Id) ON DELETE RESTRICT
);

CREATE INDEX IX_CompetencyAreas_Status ON CompetencyAreas (Status);
CREATE INDEX IX_Topics_Status ON Topics (Status);
CREATE INDEX IX_CompetencyAreaTopics_TopicId ON CompetencyAreaTopics (TopicId, CompetencyAreaId);
CREATE INDEX IX_Skills_Status ON Skills (Status);
CREATE INDEX IX_Skills_TargetLevel ON Skills (TargetLevel);
CREATE INDEX IX_SkillAssessments_SkillDate ON SkillAssessments (SkillId, AssessedAtUtc DESC);
CREATE INDEX IX_CompetencyAreaSkills_SkillId ON CompetencyAreaSkills (SkillId, CompetencyAreaId);
CREATE INDEX IX_TopicSkills_SkillId ON TopicSkills (SkillId, TopicId);
CREATE INDEX IX_Goals_Status ON Goals (Status);
CREATE INDEX IX_Goals_TargetDate ON Goals (TargetDate);
CREATE INDEX IX_Goals_UpdatedAt ON Goals (UpdatedAtUtc DESC);
CREATE INDEX IX_GoalSkills_SkillId ON GoalSkills (SkillId, GoalId);
