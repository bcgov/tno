import { IUserInfoModel } from 'hooks/api-editor';

import { IErrorModel } from '.';

export interface IAppState {
  token?: any;
  userInfo?: IUserInfoModel;
  requests: string[];
  showErrors: boolean;
  errors: IErrorModel[];
}
