CREATE TABLE KnowledgeArtifacts (
    Id TEXT NOT NULL PRIMARY KEY, Title TEXT NOT NULL, Markdown TEXT NOT NULL, Type TEXT NOT NULL,
    Status TEXT NOT NULL, CreatedAtUtc TEXT NOT NULL, UpdatedAtUtc TEXT NOT NULL, ArchivedAtUtc TEXT NULL
);
CREATE TABLE Evidence (
    Id TEXT NOT NULL PRIMARY KEY, Title TEXT NOT NULL, Description TEXT NULL, Type TEXT NOT NULL,
    OccurredAtUtc TEXT NOT NULL, Url TEXT NULL, LocalPath TEXT NULL, Evaluation TEXT NULL, Status TEXT NOT NULL,
    CreatedAtUtc TEXT NOT NULL, UpdatedAtUtc TEXT NOT NULL, ArchivedAtUtc TEXT NULL
);
CREATE TABLE KnowledgeArtifactResources (KnowledgeArtifactId TEXT NOT NULL, ResourceId TEXT NOT NULL, PRIMARY KEY(KnowledgeArtifactId, ResourceId), FOREIGN KEY(KnowledgeArtifactId) REFERENCES KnowledgeArtifacts(Id) ON DELETE CASCADE, FOREIGN KEY(ResourceId) REFERENCES Resources(Id) ON DELETE RESTRICT);
CREATE TABLE KnowledgeArtifactSkills (KnowledgeArtifactId TEXT NOT NULL, SkillId TEXT NOT NULL, PRIMARY KEY(KnowledgeArtifactId, SkillId), FOREIGN KEY(KnowledgeArtifactId) REFERENCES KnowledgeArtifacts(Id) ON DELETE CASCADE, FOREIGN KEY(SkillId) REFERENCES Skills(Id) ON DELETE RESTRICT);
CREATE TABLE KnowledgeArtifactTopics (KnowledgeArtifactId TEXT NOT NULL, TopicId TEXT NOT NULL, PRIMARY KEY(KnowledgeArtifactId, TopicId), FOREIGN KEY(KnowledgeArtifactId) REFERENCES KnowledgeArtifacts(Id) ON DELETE CASCADE, FOREIGN KEY(TopicId) REFERENCES Topics(Id) ON DELETE RESTRICT);
CREATE TABLE KnowledgeArtifactGoals (KnowledgeArtifactId TEXT NOT NULL, GoalId TEXT NOT NULL, PRIMARY KEY(KnowledgeArtifactId, GoalId), FOREIGN KEY(KnowledgeArtifactId) REFERENCES KnowledgeArtifacts(Id) ON DELETE CASCADE, FOREIGN KEY(GoalId) REFERENCES Goals(Id) ON DELETE RESTRICT);
CREATE TABLE KnowledgeArtifactLearningPaths (KnowledgeArtifactId TEXT NOT NULL, LearningPathId TEXT NOT NULL, PRIMARY KEY(KnowledgeArtifactId, LearningPathId), FOREIGN KEY(KnowledgeArtifactId) REFERENCES KnowledgeArtifacts(Id) ON DELETE CASCADE, FOREIGN KEY(LearningPathId) REFERENCES LearningPaths(Id) ON DELETE RESTRICT);
CREATE TABLE EvidenceSkills (EvidenceId TEXT NOT NULL, SkillId TEXT NOT NULL, PRIMARY KEY(EvidenceId, SkillId), FOREIGN KEY(EvidenceId) REFERENCES Evidence(Id) ON DELETE CASCADE, FOREIGN KEY(SkillId) REFERENCES Skills(Id) ON DELETE RESTRICT);
CREATE TABLE EvidenceResources (EvidenceId TEXT NOT NULL, ResourceId TEXT NOT NULL, PRIMARY KEY(EvidenceId, ResourceId), FOREIGN KEY(EvidenceId) REFERENCES Evidence(Id) ON DELETE CASCADE, FOREIGN KEY(ResourceId) REFERENCES Resources(Id) ON DELETE RESTRICT);
CREATE TABLE EvidenceGoals (EvidenceId TEXT NOT NULL, GoalId TEXT NOT NULL, PRIMARY KEY(EvidenceId, GoalId), FOREIGN KEY(EvidenceId) REFERENCES Evidence(Id) ON DELETE CASCADE, FOREIGN KEY(GoalId) REFERENCES Goals(Id) ON DELETE RESTRICT);
CREATE INDEX IX_KnowledgeArtifacts_StatusUpdated ON KnowledgeArtifacts(Status, UpdatedAtUtc DESC);
CREATE INDEX IX_Evidence_StatusOccurred ON Evidence(Status, OccurredAtUtc DESC);
CREATE INDEX IX_EvidenceSkills_Skill ON EvidenceSkills(SkillId, EvidenceId);
CREATE INDEX IX_EvidenceResources_Resource ON EvidenceResources(ResourceId, EvidenceId);
