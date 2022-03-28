import { IUserInfoModel, useApiAuth } from 'hooks/api-editor';
import React from 'react';
import { IAppState, useAppStore } from 'store/slices';

interface IAppController {
  getUserInfo: () => Promise<IUserInfoModel>;
  isUserReady: () => boolean;
}

export const useApp = (): [IAppState, IAppController] => {
  const [state, store] = useAppStore();
  const api = useApiAuth();

  const controller = React.useRef({
    getUserInfo: async () => {
      const result = await api.getUserInfo();
      store.storeUserInfo(result);
      return result;
    },
    isUserReady: () => state.userInfo !== undefined,
  });

  return [state, controller.current];
};
