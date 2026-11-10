import React from 'react';

import { useAjaxWrapper } from '@/editor/store/hooks';
import { useApiEditorUsers, type IUserModel } from '@/shared';

interface IUserController {
  getUser: (id: number) => Promise<IUserModel>;
}

export const useUsers = (): IUserController => {
  const api = useApiEditorUsers();
  const dispatch = useAjaxWrapper();

  const controller = React.useMemo(
    () => ({
      getUser: async (id: number) => {
        return (await dispatch<IUserModel>('get-user', async () => await api.getUser(id))).data;
      },
    }),
    [api, dispatch],
  );

  return controller;
};
