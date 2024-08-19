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
  updateUser: (model: ISubscriberUserModel, impersonate?: boolean) => Promise<ISubscriberUserModel>;
  getDistributionListById: (id: number) => Promise<IUserModel[]>;
}

export const useUsers = (): IUserController => {
  const { findUsers, getDistributionListById } = useApiAdminUsers();
  const { getUser, updateUser } = useApiSubscriberUsers();
  const dispatch = useAjaxWrapper();
  const [, { storeMyProfile, storeImpersonate }] = useProfileStore();
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
      updateUser: async (model: ISubscriberUserModel, impersonate?: boolean) => {
        const response = await dispatch<ISubscriberUserModel>('update-user', () =>
          updateUser(model),
        );
        impersonate ? storeImpersonate(response.data) : storeMyProfile(response.data);
        if (userInfo) storeUserInfo({ ...userInfo, preferences: response.data.preferences });
        return response.data;
      },
      getDistributionListById: async (id: number) => {
        const response = await dispatch<IUserModel[]>('get-distribution-list', () =>
          getDistributionListById(id),
        );
        return response.data;
      },
    }),
    [
      dispatch,
      getUser,
      findUsers,
      storeMyProfile,
      storeImpersonate,
      userInfo,
      storeUserInfo,
      updateUser,
      getDistributionListById,
    ],
  );

  return controller;
};
