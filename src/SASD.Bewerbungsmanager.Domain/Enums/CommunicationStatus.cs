namespace SASD.Bewerbungsmanager.Domain.Enums;

/// <summary>Represents the processing state of one imported communication message.</summary>
public enum CommunicationStatus
{
    /// <summary>The message was imported but has not yet been linked or deliberately ignored.</summary>
    Imported = 0,

    /// <summary>The message has been linked to job-search context and/or represented as an activity.</summary>
    Linked = 1,

    /// <summary>The user intentionally marked the message as irrelevant to the application tracker.</summary>
    Ignored = 2,
}
