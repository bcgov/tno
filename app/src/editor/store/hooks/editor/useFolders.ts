import React from 'react';
import { useApiEditorFolders, type IFolderContentModel } from 'tno-core';

import { useAjaxWrapper } from '@/editor/store/hooks';

interface IFolderController {
  getContentInFolder: (
    id: number,
    includeCalculatedTopicScore: boolean,
  ) => Promise<IFolderContentModel[]>;
}

export const useFolders = (): IFolderController => {
  const api = useApiEditorFolders();
  const dispatch = useAjaxWrapper();

  const controller = React.useMemo(
    () => ({
      getContentInFolder: async (id: number, includeCalculatedTopicScore: boolean = false) => {
        const response = await dispatch<IFolderContentModel[]>(
          'get-folder-content',
          async () => await api.getContentInFolder(id, includeCalculatedTopicScore),
        );
        return response.data;
      },
    }),
    [api, dispatch],
  );

  return controller;
};
