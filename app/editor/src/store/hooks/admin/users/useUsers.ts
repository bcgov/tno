import { IUserListFilter } from 'features/admin/users/interfaces/IUserListFilter';
import { IPaged, IUserModel, useApiAdminUsers } from 'hooks/api-editor';
import { IUserFilter } from 'hooks/api-editor/interfaces/IUserFilter';
import React from 'react';
import { useApiDispatcher } from 'store/hooks';
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
  const dispatch = useApiDispatcher();
  const [state, store] = useAdminStore();

  const controller = React.useMemo(
    () => ({
      findUsers: async (filter: IUserFilter) => {
        const result = await dispatch<IPaged<IUserModel>>('find-users', () =>
          api.findUsers(filter),
        );
        store.storeUsers(result);
        return result;
      },
      getUser: async (id: number) => {
        const result = await dispatch<IUserModel>('get-user', () => api.getUser(id));
        store.storeUsers({
          ...state.users,
          items: state.users.items.map((ds) => {
            if (ds.id === result.id) return result;
            return ds;
          }),
        });
        return result;
      },
      addUser: async (model: IUserModel) => {
        const result = await dispatch<IUserModel>('add-user', () => api.addUser(model));
        store.storeUsers({ ...state.users, items: [result, ...state.users.items] });
        return result;
      },
      updateUser: async (model: IUserModel) => {
        const result = await dispatch<IUserModel>('update-user', () => api.updateUser(model));
        store.storeUsers({
          ...state.users,
          items: state.users.items.map((ds) => {
            if (ds.id === result.id) return result;
            return ds;
          }),
        });
        return result;
      },
      deleteUser: async (model: IUserModel) => {
        const result = await dispatch<IUserModel>('delete-user', () => api.deleteUser(model));
        store.storeUsers({
          ...state.users,
          items: state.users.items.filter((ds) => ds.id !== result.id),
        });
        return result;
      },
      storeFilter: store.storeUserFilter,
    }),
    // The state.users will cause it to fire twice!
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [api, dispatch, store],
  );

  return [state, controller];
};
