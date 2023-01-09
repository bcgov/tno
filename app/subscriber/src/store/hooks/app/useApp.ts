import {
  IRegisterModel,
  IUserInfoModel,
  IUserModel,
  useApiAuth,
  UserStatusName,
} from 'hooks/api-editor';
import React from 'react';
import { IAppState, IErrorModel, useAppStore } from 'store/slices';
import { useKeycloakWrapper } from 'tno-core';

import { useAjaxWrapper } from '..';
import { useLookup } from '../lookup';

/**
 * Global information outside of redux so that the controller dependency doesn't cause infinite loops.
 */
let userInfo: IUserInfoModel = {
  id: 0,
  key: '',
  username: '',
  email: '',
  status: UserStatusName.Requested,
  displayName: '',
  isEnabled: false,
  roles: [],
};

let initialized = false;

interface IAppController {
  /**
   * Make an request to the API for user information.
   */
  getUserInfo: (refresh?: boolean) => Promise<IUserInfoModel>;
  requestCode: (model: IRegisterModel) => Promise<IRegisterModel>;
  requestApproval: (model: IUserModel) => Promise<IUserModel>;
  /**
   * Add new error to state.
   */
  addError: (error: IErrorModel) => void;
  /**
   * Remove specified error from state.
   */
  removeError: (error: IErrorModel) => void;
  /**
   * Clear errors from state.
   */
  clearErrors: () => void;
  /**
   * Whether the application has been initialized.
   */
  initialized: boolean;
  /**
   * Whether the application has been initialized.
   */
  authenticated: boolean;
}

/**
 * useApp is a hook that provides overall application state and an api to get user information.
 * @returns Hook with application state and api.
 */
export const useApp = (): [IAppState, IAppController] => {
  const keycloak = useKeycloakWrapper();
  const [state, store] = useAppStore();
  const [, { init }] = useLookup();
  const dispatch = useAjaxWrapper();
  const api = useApiAuth();

  const hasClaim = keycloak.hasClaim();

  React.useEffect(() => {
    // Initialize lookup values the first time the app loads.
    if (!initialized && keycloak.authenticated && hasClaim) {
      initialized = true;
      init();
    }
  }, [init, keycloak.authenticated, hasClaim]);

  const controller = React.useMemo(
    () => ({
      getUserInfo: async (refresh: boolean = false) => {
        if (userInfo.id !== 0 && !refresh) return userInfo;
        const response = await dispatch('get-user-info', () => api.getUserInfo());
        userInfo = response.data;
        store.storeUserInfo(userInfo);
        if ((!keycloak.hasClaim() || refresh) && !!response.data.roles.length)
          await keycloak.instance.updateToken(86400);
        return userInfo;
      },
      requestCode: async (model: IRegisterModel) => {
        return (await dispatch<IRegisterModel>('request-code', () => api.requestCode(model))).data;
      },
      requestApproval: async (model: IUserModel) => {
        return (await dispatch<IUserModel>('request-approval', () => api.requestApproval(model)))
          .data;
      },
      addError: store.addError,
      removeError: store.removeError,
      clearErrors: store.clearErrors,
      initialized,
      authenticated: keycloak.authenticated ?? false,
    }),
    [api, dispatch, store, keycloak],
  );

  return [state, controller];
};
