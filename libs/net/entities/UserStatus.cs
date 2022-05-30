namespace TNO.Entities;

/// <summary>
/// Provides user status options which identify whether they have been approved or not.
/// </summary>
public enum UserStatus
{
    /// <summary>
    /// User only exists in TNO, but represents a preapproved user.
    /// </summary>
    Preapproved = 0,

    /// <summary>
    /// User exists in Keycloak, but has never been activiated in TNO.
    /// </summary>
    Authenticated = 1,

    /// <summary>
    /// User has activated their account in TNO, but it has not been requested or approved.
    /// </summary>
    Activated = 2,

    /// <summary>
    /// User has requested approval to access TNO.
    /// </summary>
    Requested = 3,

    /// <summary>
    /// User has been approved to have access in TNO.
    /// </summary>
    Approved = 4,

    /// <summary>
    /// User has been denied access to TNO.
    /// </summary>
    Denied = 5
}
