import React from 'react';
import { useAjaxWrapper, useLookup } from 'store/hooks';
import { IAdminState, useAdminStore } from 'store/slices';
import { IMinisterModel, useApiAdminMinisters } from 'tno-core';

interface IMinisterController {
  findAllMinisters: () => Promise<IMinisterModel[]>;
  getMinister: (id: number) => Promise<IMinisterModel>;
  addMinister: (model: IMinisterModel) => Promise<IMinisterModel>;
  updateMinister: (model: IMinisterModel) => Promise<IMinisterModel>;
  deleteMinister: (model: IMinisterModel) => Promise<IMinisterModel>;
}

export const useMinisters = (): [IAdminState & { initialized: boolean }, IMinisterController] => {
  const api = useApiAdminMinisters();
  const dispatch = useAjaxWrapper();
  const [state, store] = useAdminStore();
  const [, lookup] = useLookup();
  const [initialized, setInitialized] = React.useState(false);

  const controller = React.useMemo(
    () => ({
      findAllMinisters: async () => {
        const response = await dispatch<IMinisterModel[]>('find-all-ministers', () =>
          api.findAllMinisters(),
        );
        store.storeMinisters(response.data);
        setInitialized(true);
        return response.data;
      },
      getMinister: async (id: number) => {
        const response = await dispatch<IMinisterModel>('get-minister', () => api.getMinister(id));
        store.storeMinisters((ministers) =>
          ministers.map((ds) => {
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
        store.storeMinisters((ministers) => [...ministers, response.data]);
        await lookup.getLookups();
        return response.data;
      },
      updateMinister: async (model: IMinisterModel) => {
        const response = await dispatch<IMinisterModel>('update-minister', () =>
          api.updateMinister(model),
        );
        store.storeMinisters((ministers) =>
          ministers.map((ds) => {
            if (ds.id === response.data.id) return response.data;
            return ds;
          }),
        );
        await lookup.getLookups();
        return response.data;
      },
      deleteMinister: async (model: IMinisterModel) => {
        const response = await dispatch<IMinisterModel>('delete-minister', () =>
          api.deleteMinister(model),
        );
        store.storeMinisters((ministers) => ministers.filter((ds) => ds.id !== response.data.id));
        await lookup.getLookups();
        return response.data;
      },
    }),
    [api, dispatch, lookup, store],
  );

  return [{ ...state, initialized }, controller];
};
