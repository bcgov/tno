import React from 'react';
import { useAjaxWrapper, useLookup } from 'store/hooks';
import { IAdminState, useAdminStore } from 'store/slices';
import { IFolderContentModel, IFolderModel, useApiAdminFolders } from 'tno-core';

interface IFolderController {
  findAllFolders: () => Promise<IFolderModel[]>;
  getFolder: (id: number, includeContent: boolean) => Promise<IFolderModel>;
  getContentInFolder: (id: number, includeMaxTopicScore: boolean) => Promise<IFolderContentModel[]>;
  addFolder: (model: IFolderModel) => Promise<IFolderModel>;
  updateFolder: (model: IFolderModel) => Promise<IFolderModel>;
  deleteFolder: (model: IFolderModel) => Promise<IFolderModel>;
}

export const useFolders = (): [IAdminState & { initialized: boolean }, IFolderController] => {
  const api = useApiAdminFolders();
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
      getFolder: async (id: number, includeContent: boolean = false) => {
        const response = await dispatch<IFolderModel>('get-folder', () =>
          api.getFolder(id, includeContent),
        );
        store.storeFolders((folders) =>
          folders.map((ds) => {
            if (ds.id === response.data.id) return response.data;
            return ds;
          }),
        );
        return response.data;
      },
      getContentInFolder: async (id: number, includeMaxTopicScore: boolean = false) => {
        const response = await dispatch<IFolderContentModel[]>('get-folder-content', () =>
          api.getContentInFolder(id, includeMaxTopicScore),
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
