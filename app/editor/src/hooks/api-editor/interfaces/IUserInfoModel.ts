export interface IUserInfoModel {
  id: number;
  username: string;
  email: string;
  displayName: string;
  firstName?: string;
  lastName?: string;
  lastLoginOn?: Date;
  roles: string[];
  groups: string[];
}
