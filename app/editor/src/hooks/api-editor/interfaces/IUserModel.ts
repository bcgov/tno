export interface IUserModel {
  id: number;
  username: string;
  email: string;
  displayName: string;
  firstName?: string;
  lastName?: string;
  lastLoginOn?: Date;
  isEnabled: boolean;
  isSystemAccount: boolean;
}
