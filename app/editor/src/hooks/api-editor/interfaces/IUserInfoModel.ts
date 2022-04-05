export interface IUserInfoModel {
  id: number;
  key: string;
  username: string;
  email: string;
  displayName: string;
  firstName?: string;
  lastName?: string;
  lastLoginOn?: Date;
  roles: string[];
  groups: string[];
}
