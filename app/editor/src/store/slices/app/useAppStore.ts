import { IUserInfoModel } from 'hooks/api-editor';
import React from 'react';
import { useAppDispatch, useAppSelector } from 'store';

import { addRequest, clearRequests, removeRequest, storeToken, storeUserInfo } from '.';
import { IAppState } from './interfaces';

export interface IAppStore {
  addRequest: (url: string) => void;
  removeRequest: (url: string) => void;
  clearRequests: () => void;
  storeUserInfo: (user?: IUserInfoModel) => void;
  storeToken: (token: any) => void;
}

export const useAppStore = (): [IAppState, IAppStore] => {
  const dispatch = useAppDispatch();
  const state = useAppSelector((store) => store.app);

  const controller = React.useMemo(
    () => ({
      addRequest: (url: string) => {
        dispatch(addRequest(url));
      },
      removeRequest: (url: string) => {
        dispatch(removeRequest(url));
      },
      clearRequests: () => {
        dispatch(clearRequests());
      },
      storeUserInfo: (user?: IUserInfoModel) => {
        dispatch(storeUserInfo(user));
      },
      storeToken: (token: any) => {
        dispatch(storeToken(token));
      },
    }),
    [dispatch],
  );

  return [state, controller];
};
