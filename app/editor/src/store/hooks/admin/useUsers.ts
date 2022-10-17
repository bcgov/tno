import { IUserListFilter } from 'features/admin/users/interfaces/IUserListFilter';
import { IPaged, IUserModel, useApiAdminUsers } from 'hooks/api-editor';
import { IUserFilter } from 'hooks/api-editor/interfaces/IUserFilter';
import React from 'react';
import { useAjaxWrapper } from 'store/hooks';
import { IAdminState, useAdminStore } from 'store/slices';

interface IUserController {
  findUsers: (filter: IUserFilter) => Promise<IPaged<IUserModel>>;
  getUser: (id: number) => Promise<IUserModel>;
  addUser: (model: IUserModel) => Promise<IUserModel>;
  updateUser: (model: IUserModel) => Promise<IUserModel>;
  deleteUser: (model: IUserModel) => Promise<IUserModel>;
  storeFilter: (filter: IUserListFilter) => void;
}

export const useUsers = (): [IAdminState, IUserController] => {
  const api = useApiAdminUsers();
  const dispatch = useAjaxWrapper();
  const [state, store] = useAdminStore();

  const controller = React.useMemo(
    () => ({
      findUsers: async (filter: IUserFilter) => {
        const response = await dispatch<IPaged<IUserModel>>('find-users', () =>
          api.findUsers(filter),
        );
        store.storeUsers(response.data);
        return response.data;
      },
      getUser: async (id: number) => {
        const response = await dispatch<IUserModel>('get-user', () => api.getUser(id));
        store.storeUsers({
          ...state.users,
          items: state.users.items.map((ds) => {
            if (ds.id === response.data.id) return response.data;
            return ds;
          }),
        });
        return response.data;
      },
      addUser: async (model: IUserModel) => {
        const response = await dispatch<IUserModel>('add-user', () => api.addUser(model));
        store.storeUsers({ ...state.users, items: [response.data, ...state.users.items] });
        return response.data;
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
      deleteUser: async (model: IUserModel) => {
        const response = await dispatch<IUserModel>('delete-user', () => api.deleteUser(model));
        store.storeUsers({
          ...state.users,
          items: state.users.items.filter((ds) => ds.id !== response.data.id),
        });
        return response.data;
      },
      storeFilter: store.storeUserFilter,
    }),
    // The state.users will cause it to fire twice!
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [api, dispatch, store],
  );

  return [state, controller];
};
