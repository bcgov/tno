import { IUserModel, useApiUsers } from 'hooks/api-editor';
import React from 'react';
import { useAjaxWrapper } from 'store/hooks';

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
