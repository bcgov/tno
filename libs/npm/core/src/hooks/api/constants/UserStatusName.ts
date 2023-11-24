export enum UserStatusName {
  /**
   * User only exists in TNO, but represents a preapproved user.
   */
  Preapproved = 'Preapproved',
  /**
   * User exists in Keycloak, but has never been activiated in TNO.
   */
  Authenticated = 'Authenticated',
  /**
   * User has activated their account in TNO, but it has not been requested or approved.
   */
  Activated = 'Activated',
  /**
   * User has requested approval to access TNO.
   */
  Requested = 'Requested',
  /**
   * User has been approved to have access in TNO.
   */
  Approved = 'Approved',
  /**
   * User has been denied access to TNO.
   */
  Denied = 'Denied',
}
