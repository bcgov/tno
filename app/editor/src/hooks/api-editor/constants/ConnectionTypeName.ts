export enum ConnectionTypeName {
  /** Files are stored on a local volume. */
  LocalVolume = 'LocalVolume',

  /** Files are stored on a remote volume. */
  RemoteVolume = 'RemoteVolume',

  /** Files are stored on a NAS. */
  NAS = 'NAS',

  /** Files are stored on the internet via URL. */
  Internet = 'Internet',

  /** Files are stored on an FTP. */
  FTP = 'FTP',

  /** Files are stored on an SFTP. */
  SFTP = 'SFTP',

  /** Files are stored on Azure. */
  Azure = 'Azure',

  /** Files are stored on an SSH. */
  SSH = 'SSH',
}
