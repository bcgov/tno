import { sortFolders } from 'features/my-folders/utils';
import React from 'react';
import { useAjaxWrapper } from 'store/hooks';
import { IProfileState, useProfileStore } from 'store/slices';
import { IFolderModel, useApiSubscriberFolders } from 'tno-core';

interface IFolderController {
  findMyFolders: () => Promise<IFolderModel[]>;
  getFolder: (id: number, includeContent: boolean) => Promise<IFolderModel>;
  addFolder: (model: IFolderModel) => Promise<IFolderModel>;
  updateFolder: (model: IFolderModel, updateContent: boolean) => Promise<IFolderModel>;
  deleteFolder: (model: IFolderModel) => Promise<IFolderModel>;
}

export const useFolders = (): [IProfileState, IFolderController] => {
  const api = useApiSubscriberFolders();
  const dispatch = useAjaxWrapper();
  const [state, { storeMyFolders }] = useProfileStore();

  const controller = React.useMemo(
    () => ({
      findMyFolders: async () => {
        const response = await dispatch<IFolderModel[]>('find-my-folders', () =>
          api.findMyFolders(),
        );
        storeMyFolders(response.data);
        return response.data;
      },
      getFolder: async (id: number, includeContent: boolean = false) => {
        const response = await dispatch<IFolderModel>('get-folder', () =>
          api.getFolder(id, includeContent),
        );
        storeMyFolders((folders) => {
          let exists = false;
          let results = folders.map((f) => {
            if (f.id === response.data.id) exists = true;
            return f.id === response.data.id ? response.data : f;
          });
          if (!exists) results = [...folders, response.data];
          return results;
        });
        return response.data;
      },
      addFolder: async (model: IFolderModel) => {
        const response = await dispatch<IFolderModel>('add-folder', () => api.addFolder(model));
        storeMyFolders((folders) => sortFolders([...folders, response.data]));
        return response.data;
      },
      updateFolder: async (model: IFolderModel, updateContent: boolean) => {
        const response = await dispatch<IFolderModel>('update-folder', () =>
          api.updateFolder(model, updateContent),
        );
        storeMyFolders((folders) =>
          sortFolders(folders.map((ds) => (ds.id === response.data.id ? response.data : ds))),
        );
        return response.data;
      },
      deleteFolder: async (model: IFolderModel) => {
        const response = await dispatch<IFolderModel>('delete-folder', () =>
          api.deleteFolder(model),
        );
        storeMyFolders((folders) => folders.filter((ds) => ds.id !== response.data.id));
        return response.data;
      },
    }),
    [api, dispatch, storeMyFolders],
  );

  return [state, controller];
};
