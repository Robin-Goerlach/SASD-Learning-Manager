namespace SASD.Bewerbungsmanager.Domain.Enums;

/// <summary>Describes whether a communication message was received or sent by the user.</summary>
public enum CommunicationDirection
{
    /// <summary>The message was received from another person or system.</summary>
    Incoming = 0,

    /// <summary>The message was sent by the user.</summary>
    Outgoing = 1,
}
