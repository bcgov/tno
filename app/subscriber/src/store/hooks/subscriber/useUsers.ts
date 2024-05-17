import React from 'react';
import { useAjaxWrapper } from 'store/hooks';
import { useAppStore, useProfileStore } from 'store/slices';
import {
  IPaged,
  ISubscriberUserModel,
  IUserFilter,
  IUserModel,
  useApiAdminUsers,
  useApiSubscriberUsers,
} from 'tno-core';

interface IUserController {
  getUser: () => Promise<ISubscriberUserModel>;
  findUsers: (filter: IUserFilter) => Promise<IPaged<IUserModel>>;
  updateUser: (model: ISubscriberUserModel) => Promise<ISubscriberUserModel>;
}

export const useUsers = (): IUserController => {
  const { findUsers } = useApiAdminUsers();
  const { getUser, updateUser } = useApiSubscriberUsers();
  const dispatch = useAjaxWrapper();
  const [, { storeMyProfile }] = useProfileStore();
  const [{ userInfo }, { storeUserInfo }] = useAppStore();

  const controller = React.useMemo(
    () => ({
      getUser: async () => {
        const response = await dispatch('get-user', () => getUser());
        return response.data;
      },
      findUsers: async (filter: IUserFilter) => {
        const response = await dispatch('find-users', () => findUsers(filter));
        return response.data;
      },
      updateUser: async (model: ISubscriberUserModel) => {
        const response = await dispatch<ISubscriberUserModel>('update-user', () =>
          updateUser(model),
        );
        storeMyProfile(response.data);
        if (userInfo) storeUserInfo({ ...userInfo, preferences: response.data.preferences });
        return response.data;
      },
    }),
    [dispatch, getUser, findUsers, storeMyProfile, userInfo, storeUserInfo, updateUser],
  );

  return controller;
};
