import React from 'react';
import { IAppState, IErrorModel, IUserOptions, useAppStore } from 'store/slices';
import {
  AccountAuthStateName,
  getFromLocalStorage,
  IRegisterModel,
  IUserInfoModel,
  IUserModel,
  saveToLocalStorage,
  useApiAuth,
  useKeycloakWrapper,
  UserStatusName,
} from 'tno-core';

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
  preferredEmail: '',
  status: UserStatusName.Requested,
  displayName: '',
  isEnabled: false,
  roles: [],
  authState: AccountAuthStateName.Authorized,
  mediaTypes: [],
};

let initialized = false;

interface IAppController {
  getUserOptions: (refresh?: boolean) => Promise<IUserOptions>;
  storeUserOptions: (options: IUserOptions) => void;
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
    controller.getUserOptions().catch(() => {});
    // Only init on startup.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  React.useEffect(() => {
    // Initialize lookup values the first time the app loads.
    if (!initialized && keycloak.authenticated && hasClaim) {
      initialized = true;
      init();
    }
  }, [init, keycloak.authenticated, hasClaim]);

  const controller = React.useMemo(
    () => ({
      getUserOptions: async (refresh: boolean = false) => {
        if (!!state.options && !refresh) return state.options;

        const options = getFromLocalStorage<IUserOptions>('options', {});
        store.storeUserOptions(options);
        return Promise.resolve(options);
      },
      storeUserOptions: (options: IUserOptions) => {
        saveToLocalStorage('options', options);
        store.storeUserOptions(options);
      },
      getUserInfo: async (refresh: boolean = false) => {
        try {
          if (userInfo.id !== 0 && !refresh) return userInfo;
          const response = await dispatch('get-user-info', () => api.getUserInfo());
          userInfo = response.data;
          store.storeUserInfo(userInfo);
          if ((!keycloak.hasClaim() || refresh) && !!response.data.roles.length)
            await keycloak.instance.updateToken(86400);
          return userInfo;
        } catch (error) {
          throw error;
        }
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
    [store, keycloak, state.options, dispatch, api],
  );

  return [state, controller];
};
