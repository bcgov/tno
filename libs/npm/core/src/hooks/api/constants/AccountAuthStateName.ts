export enum AccountAuthStateName {
  /// <summary>
  /// This device is unauthorized and must be blocked.
  /// </summary>
  Unauthorized = 'Unauthorized',
  /// <summary>
  /// This device is authorized.
  /// </summary>
  Authorized = 'Authorized',
  /// <summary>
  /// This account is logged into multiple IPs at the same time.
  /// </summary>
  MultipleIPs = 'MultipleIPs',
  /// <summary>
  /// This account is logged into multiple devices at the same time.
  /// </summary>
  MultipleDevices = 'MultipleDevices',
}
