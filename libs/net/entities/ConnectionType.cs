namespace TNO.Entities;

/// <summary>
/// Provides connection type.
/// </summary>
public enum ConnectionType
{
    /// <summary>
    /// Files are stored on a local volume.
    /// </summary>
    LocalVolume = 0,

    /// <summary>
    /// Files are stored on a NAS.
    /// </summary>
    NAS = 1,

    /// <summary>
    /// Files are stored on the internet via URL.
    /// </summary>
    HTTP = 2,

    /// <summary>
    /// Files are stored on an FTP.
    /// </summary>
    FTP = 3,

    /// <summary>
    /// Files are stored on an SFTP.
    /// </summary>
    SFTP = 4,

    /// <summary>
    /// Files are stored on Azure.
    /// </summary>
    Azure = 5,

    /// <summary>
    /// Files are stored on AWS.
    /// </summary>
    AWS = 6,

    /// <summary>
    /// Files are accessible via SSH.
    /// </summary>
    SSH = 7,

    /// <summary>
    /// Files are accessible via SSH.
    /// </summary>
    Database = 8,
}
