export enum ConnectionType {
  /** Files are stored on a local volume. */
  LocalVolume = 0,

  /** Files are stored on a NAS. */
  NAS = 1,

  /** Files are stored on the internet via URL. */
  HTTP = 2,

  /** Files are stored on an FTP. */
  FTP = 3,

  /** Files are stored on an SFTP. */
  SFTP = 4,

  /** Files are stored on Azure. */
  Azure = 5,

  /** Files are stored on AWS. */
  AWS = 6,

  /** Files are stored on an SSH. */
  SSH = 7,
}
