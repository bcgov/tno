import { IUserInfoModel, useApiAuth } from 'hooks';
import React from 'react';
import { IAppState, IAppStore, useAppStore } from 'store/slices';

interface IAppHook {
  getUserInfo: () => Promise<IUserInfoModel>;
  isUserReady: () => boolean;
}

export const useApp = (): [IAppState, IAppHook, IAppStore] => {
  const [state, store] = useAppStore();
  const api = useApiAuth();

  const hook: IAppHook = React.useMemo(
    () => ({
      getUserInfo: async () => {
        const result = await api.getUserInfo();
        store.storeUserInfo(result);
        return result;
      },
      isUserReady: () => state.userInfo !== undefined,
    }),
    [api, store, state.userInfo],
  );

  return [state, hook, store];
};
