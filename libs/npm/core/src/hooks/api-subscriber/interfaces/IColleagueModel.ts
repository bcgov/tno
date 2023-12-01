import { IUserModel } from 'hooks';

export interface IColleagueModel {
  userId: number;
  user: IUserModel;
  colleagueId: number;
  colleague: IUserModel;
}
