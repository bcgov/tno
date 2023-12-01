export interface IUserModel {
  id: number;
  key: string;
  username: string;
  email: string;
  displayName: string;
  firstName: string;
  lastName: string;
  lastLoginOn?: Date;
  isEnabled: boolean;
}
