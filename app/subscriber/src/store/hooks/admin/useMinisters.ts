import React from 'react';
import { useAjaxWrapper } from 'store/hooks';
import { IAdminState, useAdminStore } from 'store/slices';
import { IMinisterModel, useApiAdminMinisters } from 'tno-core';

interface IMinisterController {
  findAllMinisters: () => Promise<IMinisterModel[]>;
  getMinister: (id: number) => Promise<IMinisterModel>;
  addMinister: (model: IMinisterModel) => Promise<IMinisterModel>;
  updateMinister: (model: IMinisterModel) => Promise<IMinisterModel>;
  deleteMinister: (model: IMinisterModel) => Promise<IMinisterModel>;
}

export const useMinisters = (): [IAdminState, IMinisterController] => {
  const api = useApiAdminMinisters();
  const dispatch = useAjaxWrapper();
  const [state, store] = useAdminStore();

  const controller = React.useMemo(
    () => ({
      findAllMinisters: async () => {
        const response = await dispatch<IMinisterModel[]>('find-all-ministers', () =>
          api.findAllMinisters(),
        );
        store.storeMinisters(response.data);
        return response.data;
      },
      getMinister: async (id: number) => {
        const response = await dispatch<IMinisterModel>('get-minister', () => api.getMinister(id));
        store.storeMinisters(
          state.ministers.map((ds) => {
            if (ds.id === response.data.id) return response.data;
            return ds;
          }),
        );
        return response.data;
      },
      addMinister: async (model: IMinisterModel) => {
        const response = await dispatch<IMinisterModel>('add-minister', () =>
          api.addMinister(model),
        );
        store.storeMinisters([...state.ministers, response.data]);
        return response.data;
      },
      updateMinister: async (model: IMinisterModel) => {
        const response = await dispatch<IMinisterModel>('update-minister', () =>
          api.updateMinister(model),
        );
        store.storeMinisters(
          state.ministers.map((ds) => {
            if (ds.id === response.data.id) return response.data;
            return ds;
          }),
        );
        return response.data;
      },
      deleteMinister: async (model: IMinisterModel) => {
        const response = await dispatch<IMinisterModel>('delete-minister', () =>
          api.deleteMinister(model),
        );
        store.storeMinisters(state.ministers.filter((ds) => ds.id !== response.data.id));
        return response.data;
      },
    }),
    // The state.ministers will cause it to fire twice!
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [dispatch, store, api],
  );

  return [state, controller];
};
