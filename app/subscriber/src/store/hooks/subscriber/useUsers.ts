import React from 'react';
import { useAjaxWrapper } from 'store/hooks';
import { useAdminStore } from 'store/slices';
import { IUserModel } from 'tno-core';

import { useApiUsers } from './api/useApiUsers';

interface IUserController {
  updateUser: (model: IUserModel, requestorId: number) => Promise<IUserModel>;
}

export const useUsers = (): IUserController => {
  const api = useApiUsers();
  const dispatch = useAjaxWrapper();
  const [state, store] = useAdminStore();

  const controller = React.useMemo(
    () => ({
      updateUser: async (model: IUserModel) => {
        const response = await dispatch<IUserModel>('update-user', () => api.updateUser(model));
        store.storeUsers({
          ...state.users,
          items: state.users.items.map((ds) => {
            if (ds.id === response.data.id) return response.data;
            return ds;
          }),
        });
        return response.data;
      },
    }),
    [api, dispatch, state.users, store],
  );

  return controller;
};
