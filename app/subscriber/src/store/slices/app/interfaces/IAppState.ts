import { IUserInfoModel } from 'hooks/api-editor';

import { IAjaxRequest, IErrorModel } from '.';

export interface IAppState {
  /** The current Java Web Token */
  token?: any;
  /** The current user information. */
  userInfo?: IUserInfoModel;
  /** An array of active AJAX requests. */
  requests: IAjaxRequest[];
  /** Whether to display errors. */
  showErrors: boolean;
  /** An array of errors. */
  errors: IErrorModel[];
}
