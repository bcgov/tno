import React from 'react';
import { useAjaxWrapper } from 'store/hooks';
import { IProfileState, useProfileStore } from 'store/slices';
import { IFolderModel, useApiSubscriberFolders } from 'tno-core';

interface IFolderController {
  findAllFolders: () => Promise<IFolderModel[]>;
  findMyFolders: () => Promise<IFolderModel[]>;
  getFolder: (id: number, includeContent: boolean) => Promise<IFolderModel>;
  addFolder: (model: IFolderModel) => Promise<IFolderModel>;
  updateFolder: (model: IFolderModel) => Promise<IFolderModel>;
  deleteFolder: (model: IFolderModel) => Promise<IFolderModel>;
}

export const useFolders = (): [IProfileState, IFolderController] => {
  const api = useApiSubscriberFolders();
  const dispatch = useAjaxWrapper();
  const [state, store] = useProfileStore();

  const controller = React.useMemo(
    () => ({
      findAllFolders: async () => {
        const response = await dispatch<IFolderModel[]>('find-all-folders', () =>
          api.findAllFolders(),
        );
        store.storeMyFolders(response.data);
        return response.data;
      },
      findMyFolders: async () => {
        const response = await dispatch<IFolderModel[]>('find-my-folders', () =>
          api.findMyFolders(),
        );
        store.storeMyFolders(response.data);
        return response.data;
      },
      getFolder: async (id: number, includeContent: boolean = false) => {
        const response = await dispatch<IFolderModel>('get-folder', () =>
          api.getFolder(id, includeContent),
        );
        store.storeMyFolders((folders) =>
          folders.map((ds) => {
            if (ds.id === response.data.id) return response.data;
            return ds;
          }),
        );
        return response.data;
      },
      addFolder: async (model: IFolderModel) => {
        const response = await dispatch<IFolderModel>('add-folder', () => api.addFolder(model));
        store.storeMyFolders((folders) => [...folders, response.data]);
        return response.data;
      },
      updateFolder: async (model: IFolderModel) => {
        const response = await dispatch<IFolderModel>('update-folder', () =>
          api.updateFolder(model),
        );
        store.storeMyFolders((folders) =>
          folders.map((ds) => {
            if (ds.id === response.data.id) return response.data;
            return ds;
          }),
        );
        return response.data;
      },
      deleteFolder: async (model: IFolderModel) => {
        const response = await dispatch<IFolderModel>('delete-folder', () =>
          api.deleteFolder(model),
        );
        store.storeMyFolders((folders) => folders.filter((ds) => ds.id !== response.data.id));
        return response.data;
      },
    }),
    [api, dispatch, store],
  );

  return [state, controller];
};
