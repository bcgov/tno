import { IUserModel } from './IUserModel';

export interface IUserColleagueModel {
  userId: number;
  colleagueId: number;
  colleague?: IUserModel;
}
