import { HubConnectionState } from '@microsoft/signalr';
import React from 'react';
import { useAppDispatch, useAppSelector } from 'store';
import { IUserInfoModel } from 'tno-core';

import {
  addError,
  addRequest,
  changeHubState,
  clearErrors,
  clearRequests,
  removeError,
  removeRequest,
  storeUserInfo,
  storeUserOptions,
} from '.';
import { IAppState, IErrorModel, IUserOptions } from './interfaces';

export interface IAppStore {
  changeHubState: (state: HubConnectionState) => void;
  storeUserOptions: (options?: IUserOptions) => void;
  storeUserInfo: (user?: IUserInfoModel) => void;
  addRequest: (url: string, group?: string | string[], isSilent?: boolean) => void;
  removeRequest: (url: string) => void;
  clearRequests: () => void;
  addError: (error: IErrorModel) => void;
  removeError: (error: IErrorModel) => void;
  clearErrors: () => void;
}

/**
 * Hook
 * @returns Array with state and controller.
 */
export const useAppStore = (): [IAppState, IAppStore] => {
  const dispatch = useAppDispatch();
  const state = useAppSelector((store) => store.app);

  const controller = React.useMemo(
    () => ({
      changeHubState: (state: HubConnectionState) => {
        dispatch(changeHubState(state));
      },
      storeUserOptions: (options?: IUserOptions) => {
        dispatch(storeUserOptions(options));
      },
      storeUserInfo: (user?: IUserInfoModel) => {
        dispatch(storeUserInfo(user));
      },
      addRequest: (
        url: string,
        group: string | string[] | undefined = undefined,
        isSilent: boolean = false,
      ) => {
        var groups = Array.isArray(group) ? group : !!group ? [group] : [];
        dispatch(addRequest({ url, group: groups, isSilent }));
      },
      removeRequest: (url: string) => {
        dispatch(removeRequest(url));
      },
      clearRequests: () => {
        dispatch(clearRequests());
      },
      addError: (error: IErrorModel) => {
        dispatch(addError(error));
      },
      removeError: (error: IErrorModel) => {
        dispatch(removeError(error));
      },
      clearErrors: () => {
        dispatch(clearErrors());
      },
    }),
    [dispatch],
  );

  return [state, controller];
};
