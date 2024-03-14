import React from 'react';
import { useAjaxWrapper } from 'store/hooks';
import { useProfileStore } from 'store/slices';
import { ISubscriberUserModel, useApiSubscriberUsers } from 'tno-core';

interface IUserController {
  updateUser: (model: ISubscriberUserModel, requestorId: number) => Promise<ISubscriberUserModel>;
}

export const useUsers = (): IUserController => {
  const api = useApiSubscriberUsers();
  const dispatch = useAjaxWrapper();
  const [, store] = useProfileStore();

  const controller = React.useMemo(
    () => ({
      updateUser: async (model: ISubscriberUserModel) => {
        const response = await dispatch<ISubscriberUserModel>('update-user', () =>
          api.updateUser(model),
        );
        store.storeMyProfile(response.data);
        return response.data;
      },
    }),
    [api, dispatch, store],
  );

  return controller;
};
