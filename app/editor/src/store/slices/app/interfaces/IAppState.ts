import { IUserInfoModel } from 'hooks/api-editor';

export interface IAppState {
  token?: any;
  requests: string[];
  userInfo?: IUserInfoModel;
}
