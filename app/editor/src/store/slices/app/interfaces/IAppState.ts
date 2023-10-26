import { HubConnectionState } from '@microsoft/signalr';
import { IUserInfoModel } from 'tno-core';

import { IAjaxRequest, IErrorModel, IUserOptions } from '.';

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
  /** User preference options */
  options?: IUserOptions;
  /** SignalR hub state */
  hubState: HubConnectionState;
}
