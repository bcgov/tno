import { IRegisterModel, IUserModel, useApiUsers } from 'hooks/api-editor';
import React from 'react';
import { useApiDispatcher } from 'store/hooks';

interface IUserController {
  getUser: (id: number) => Promise<IUserModel>;
  requestCode: (model: IRegisterModel) => Promise<IRegisterModel>;
  requestApproval: (model: IUserModel) => Promise<IUserModel>;
}

export const useUsers = (): IUserController => {
  const api = useApiUsers();
  const dispatch = useApiDispatcher();

  const controller = React.useMemo(
    () => ({
      getUser: async (id: number) => {
        return await dispatch<IUserModel>('get-user', () => api.getUser(id));
      },
      requestCode: async (model: IRegisterModel) => {
        return await dispatch<IRegisterModel>('request-code', () => api.requestCode(model));
      },
      requestApproval: async (model: IUserModel) => {
        return await dispatch<IUserModel>('request-approval', () => api.requestApproval(model));
      },
    }),
    [api, dispatch],
  );

  return controller;
};
