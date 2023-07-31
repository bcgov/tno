import React from 'react';
import { useAjaxWrapper, useLookup } from 'store/hooks';
import { IAdminState, useAdminStore } from 'store/slices';
import { IFolderModel } from 'tno-core';

import { useApiFolders } from './api';

interface IFolderController {
  findAllFolders: () => Promise<IFolderModel[]>;
  findMyFolders: () => Promise<IFolderModel[]>;
  getFolder: (id: number) => Promise<IFolderModel>;
  addFolder: (model: IFolderModel) => Promise<IFolderModel>;
  updateFolder: (model: IFolderModel) => Promise<IFolderModel>;
  deleteFolder: (model: IFolderModel) => Promise<IFolderModel>;
}

export const useFolders = (): [IAdminState & { initialized: boolean }, IFolderController] => {
  const api = useApiFolders();
  const dispatch = useAjaxWrapper();
  const [state, store] = useAdminStore();
  const [, lookup] = useLookup();
  const [initialized, setInitialized] = React.useState(false);

  const controller = React.useMemo(
    () => ({
      findAllFolders: async () => {
        const response = await dispatch<IFolderModel[]>('find-all-folders', () =>
          api.findAllFolders(),
        );
        store.storeFolders(response.data);
        setInitialized(true);
        return response.data;
      },
      findMyFolders: async () => {
        const response = await dispatch<IFolderModel[]>('find-my-folders', () =>
          api.findMyFolders(),
        );
        store.storeFolders(response.data);
        setInitialized(true);
        return response.data;
      },
      getFolder: async (id: number) => {
        const response = await dispatch<IFolderModel>('get-folder', () => api.getFolder(id));
        store.storeFolders((folders) =>
          folders.map((ds) => {
            if (ds.id === response.data.id) return response.data;
            return ds;
          }),
        );
        return response.data;
      },
      addFolder: async (model: IFolderModel) => {
        const response = await dispatch<IFolderModel>('add-folder', () => api.addFolder(model));
        store.storeFolders((folders) => [...folders, response.data]);
        await lookup.getLookups();
        return response.data;
      },
      updateFolder: async (model: IFolderModel) => {
        const response = await dispatch<IFolderModel>('update-folder', () =>
          api.updateFolder(model),
        );
        store.storeFolders((folders) =>
          folders.map((ds) => {
            if (ds.id === response.data.id) return response.data;
            return ds;
          }),
        );
        await lookup.getLookups();
        return response.data;
      },
      deleteFolder: async (model: IFolderModel) => {
        const response = await dispatch<IFolderModel>('delete-folder', () =>
          api.deleteFolder(model),
        );
        store.storeFolders((folders) => folders.filter((ds) => ds.id !== response.data.id));
        await lookup.getLookups();
        return response.data;
      },
    }),
    [api, dispatch, lookup, store],
  );

  return [{ ...state, initialized }, controller];
};
