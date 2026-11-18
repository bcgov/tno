/**
 * Claim, provides the claim identifiers used for permissions.
 */
export enum Claim {
  /** Has administrative privileges */
  administrator = 'administrator',
  /** Has an approved subscriber account */
  subscriber = 'subscriber',
  /** Can perform editing activities */
  editor = 'editor',
}
