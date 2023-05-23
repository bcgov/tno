import React from 'react';
import { useAjaxWrapper } from 'store/hooks';
import { IUserModel } from 'tno-core';
import { useApiUsers } from './api/useApiUsers';
import { useAdminStore } from 'store/slices';

interface IUserController {
  getUser: (id: number) => Promise<IUserModel>;
  updateUser: (model: IUserModel) => Promise<IUserModel>;
}

export const useUsers = (): IUserController => {
  const api = useApiUsers();
  const dispatch = useAjaxWrapper();
  const [state, store] = useAdminStore();

  const controller = React.useMemo(
    () => ({
      getUser: async (id: number) => {
        return (await dispatch<IUserModel>('get-user', () => api.getUser(id))).data;
      },
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
    [api, dispatch],
  );

  return controller;
};
