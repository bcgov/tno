export enum UserStatus {
  /**
   * User only exists in TNO, but represents a preapproved user.
   */
  Preapproved = 0,
  /**
   * User exists in Keycloak, but has never been activated in TNO.
   */
  Authenticated = 1,
  /**
   * User has activated their account in TNO, but it has not been requested or approved.
   */
  Activated = 2,
  /**
   * User has requested approval to access TNO.
   */
  Requested = 3,
  /**
   * User has been approved to have access in TNO.
   */
  Approved = 4,
  /**
   * User has been denied access to TNO.
   */
  Denied = 5,
}
