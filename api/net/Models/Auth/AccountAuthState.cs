namespace TNO.API.Models.Auth;

/// <summary>
/// AccountAuthState enum, provides a way to identify the authorization state of an account.
/// </summary>
public enum AccountAuthState
{
    /// <summary>
    /// This device is unauthorized and must be blocked.
    /// </summary>
    Unauthorized = 0,
    /// <summary>
    /// This device is authorized.
    /// </summary>
    Authorized = 1,
    /// <summary>
    /// This account is logged into multiple IPs at the same time.
    /// </summary>
    MultipleIPs = 2,
    /// <summary>
    /// This account is logged into multiple devices at the same time.
    /// </summary>
    MultipleDevices = 3,
}
