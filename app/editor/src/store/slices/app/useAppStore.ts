import { IUserInfoModel } from 'hooks/api-editor';
import React from 'react';
import { useAppDispatch, useAppSelector } from 'store';

import {
  addError,
  addRequest,
  clearErrors,
  clearRequests,
  removeError,
  removeRequest,
  storeToken,
  storeUserInfo,
} from '.';
import { IAppState, IErrorModel } from './interfaces';

export interface IAppStore {
  storeToken: (token: any) => void;
  storeUserInfo: (user?: IUserInfoModel) => void;
  addRequest: (url: string) => void;
  removeRequest: (url: string) => void;
  clearRequests: () => void;
  addError: (error: IErrorModel) => void;
  removeError: (error: IErrorModel) => void;
  clearErrors: () => void;
}

export const useAppStore = (): [IAppState, IAppStore] => {
  const dispatch = useAppDispatch();
  const state = useAppSelector((store) => store.app);

  const controller = React.useMemo(
    () => ({
      storeToken: (token: any) => {
        dispatch(storeToken(token));
      },
      storeUserInfo: (user?: IUserInfoModel) => {
        dispatch(storeUserInfo(user));
      },
      addRequest: (url: string) => {
        dispatch(addRequest(url));
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
