import React from 'react';
import { useAjaxWrapper } from 'store/hooks';
import { IUserModel, useApiUsers } from 'tno-core';

interface IUserController {
  getUser: (id: number) => Promise<IUserModel>;
}

export const useUsers = (): IUserController => {
  const api = useApiUsers();
  const dispatch = useAjaxWrapper();

  const controller = React.useMemo(
    () => ({
      getUser: async (id: number) => {
        return (await dispatch<IUserModel>('get-user', () => api.getUser(id))).data;
      },
    }),
    [api, dispatch],
  );

  return controller;
};
