import React from 'react';
import { useAjaxWrapper } from 'store/hooks';
import { useProfileStore } from 'store/slices';
import { IUserModel } from 'tno-core';

import { useApiUsers } from './api/useApiUsers';

interface IUserController {
  updateUser: (model: IUserModel, requestorId: number) => Promise<IUserModel>;
}

export const useUsers = (): IUserController => {
  const api = useApiUsers();
  const dispatch = useAjaxWrapper();
  const [, store] = useProfileStore();

  const controller = React.useMemo(
    () => ({
      updateUser: async (model: IUserModel) => {
        const response = await dispatch<IUserModel>('update-user', () => api.updateUser(model));
        store.storeMyProfile(response.data);
        return response.data;
      },
    }),
    [api, dispatch, store],
  );

  return controller;
};
