import { type HubConnectionState } from '@microsoft/signalr';
import { type IUserInfoModel } from 'tno-core';

import { type IAjaxRequest, type IErrorModel, type IUserOptions } from '.';

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
