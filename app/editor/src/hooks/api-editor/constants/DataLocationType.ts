export enum DataLocationType {
  /** Files are stored on a local volume. */
  LocalVolume = 0,

  /** Files are stored on a remote volume. */
  RemoteVolume = 1,

  /** Files are stored on a NAS. */
  NAS = 2,

  /** Files are stored on the internet via URL. */
  Internet = 3,

  /** Files are stored on an FTP. */
  FTP = 4,

  /** Files are stored on an SFTP. */
  SFTP = 5,

  /** Files are stored on Azure. */
  Azure = 6,

  /** Files are stored on an SSH. */
  SSH = 7,
}
