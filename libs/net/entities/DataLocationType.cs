namespace TNO.Entities;

/// <summary>
/// Provides data location type.
/// </summary>
public enum DataLocationType
{
    /// <summary>
    /// Files are stored on a local volume.
    /// </summary>
    LocalVolume = 0,

    /// <summary>
    /// Files are stored on a remote volume.
    /// </summary>
    RemoteVolume = 1,

    /// <summary>
    /// Files are stored on a NAS.
    /// </summary>
    NAS = 2,

    /// <summary>
    /// Files are stored on the internet via URL.
    /// </summary>
    Internet = 3,

    /// <summary>
    /// Files are stored on an FTP.
    /// </summary>
    FTP = 4,

    /// <summary>
    /// Files are stored on an SFTP.
    /// </summary>
    SFTP = 5,

    /// <summary>
    /// Files are stored on Azure.
    /// </summary>
    Azure = 6,
}
