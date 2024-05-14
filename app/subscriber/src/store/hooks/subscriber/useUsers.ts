import React from 'react';
import { useAjaxWrapper } from 'store/hooks';
import { useAppStore, useProfileStore } from 'store/slices';
import { ISubscriberUserModel, useApiSubscriberUsers } from 'tno-core';

interface IUserController {
  updateUser: (model: ISubscriberUserModel) => Promise<ISubscriberUserModel>;
}

export const useUsers = (): IUserController => {
  const api = useApiSubscriberUsers();
  const dispatch = useAjaxWrapper();
  const [, { storeMyProfile }] = useProfileStore();
  const [{ userInfo }, { storeUserInfo }] = useAppStore();

  const controller = React.useMemo(
    () => ({
      updateUser: async (model: ISubscriberUserModel) => {
        const response = await dispatch<ISubscriberUserModel>('update-user', () =>
          api.updateUser(model),
        );
        storeMyProfile(response.data);
        if (userInfo) storeUserInfo({ ...userInfo, preferences: response.data.preferences });
        return response.data;
      },
    }),
    [api, dispatch, storeMyProfile, storeUserInfo, userInfo],
  );

  return controller;
};
