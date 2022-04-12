import { IUserInfoModel, useApiAuth } from 'hooks/api-editor';
import React from 'react';
import { IAppState, IErrorModel, useAppStore } from 'store/slices';

import { useApiDispatcher } from '..';

/**
 * Global information outside of redux so that the controller dependency doesn't cause infinite loops.
 */
let userInfo: IUserInfoModel = {
  id: 0,
  key: '',
  username: '',
  email: '',
  displayName: '',
  roles: [],
  groups: [],
};

interface IAppController {
  /**
   * Make an request to the API for user information.
   */
  getUserInfo: () => Promise<IUserInfoModel>;
  /**
   * Check if the user is ready.
   */
  isUserReady: () => boolean;
  /**
   * Remove specified error from state.
   */
  removeError: (error: IErrorModel) => void;
  /**
   * Clear errors from state.
   */
  clearErrors: () => void;
}

/**
 * useApp is a hook that provides overall application state and an api to get user information.
 * @returns Hook with application state and api.
 */
export const useApp = (): [IAppState, IAppController] => {
  const [state, store] = useAppStore();
  const dispatch = useApiDispatcher();
  const api = useApiAuth();

  const controller = React.useMemo(
    () => ({
      getUserInfo: async () => {
        const result = await dispatch('get-user-info', () => api.getUserInfo());
        userInfo = result;
        store.storeUserInfo(result);
        return result;
      },
      isUserReady: () => userInfo.id !== 0,
      removeError: store.removeError,
      clearErrors: store.clearErrors,
    }),
    [api, dispatch, store],
  );

  return [state, controller];
};
