import React from 'react';
import { useAjaxWrapper } from 'store/hooks';
import { type IFolderContentModel, useApiEditorFolders } from 'tno-core';

interface IFolderController {
  getContentInFolder: (id: number, includeMaxTopicScore: boolean) => Promise<IFolderContentModel[]>;
}

export const useFolders = (): IFolderController => {
  const api = useApiEditorFolders();
  const dispatch = useAjaxWrapper();

  const controller = React.useMemo(
    () => ({
      getContentInFolder: async (id: number, includeMaxTopicScore: boolean = false) => {
        const response = await dispatch<IFolderContentModel[]>(
          'get-folder-content',
          async () => await api.getContentInFolder(id, includeMaxTopicScore),
        );
        return response.data;
      },
    }),
    [api, dispatch],
  );

  return controller;
};
