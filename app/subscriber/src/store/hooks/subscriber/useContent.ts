import {
  IContentListAdvancedFilter,
  IContentListFilter,
} from 'features/content/list-view/interfaces';
import React from 'react';
import { useContentStore } from 'store/slices';
import { IContentProps, IContentState } from 'store/slices/content';
import { IContentFilter, IContentModel, IPaged, useApiSubscriberContents } from 'tno-core';

import { useAjaxWrapper } from '..';

interface IContentController {
  findContent: (filter: IContentFilter) => Promise<IPaged<IContentModel>>;
  getContent: (id: number) => Promise<IContentModel | undefined>;
  getFrontPages: () => Promise<IPaged<IContentModel>>;
  download: (id: number, fileName: string) => Promise<unknown>;
  storeFilter: (filter: IContentListFilter) => void;
  storeFilterAdvanced: (filter: IContentListAdvancedFilter) => void;
  stream: (path: string) => Promise<string>;
}

export const useContent = (props?: IContentProps): [IContentState, IContentController] => {
  const [state, actions] = useContentStore(props);
  const dispatch = useAjaxWrapper();
  const api = useApiSubscriberContents();

  const controller = React.useMemo(
    () => ({
      findContent: async (filter: IContentFilter) => {
        const response = await dispatch('find-contents', () => api.findContent(filter));
        actions.storeContent(response.data);
        return response.data;
      },
      getFrontPages: async () => {
        const response = await dispatch('find-contents', () => api.getFrontPages());
        return response.data;
      },
      getContent: async (id: number) => {
        const response = await dispatch('get-content', () => api.getContent(id), 'content');
        return response.status === 204 ? undefined : response.data;
      },
      download: async (id: number, fileName: string) => {
        return (await dispatch('download-content', () => api.download(id, fileName), 'content'))
          .data;
      },
      stream: async (path: string) => {
        return (await dispatch('stream-content', () => api.stream(path), 'content')).data;
      },
      storeFilter: actions.storeFilter,
      storeFilterAdvanced: actions.storeFilterAdvanced,
    }),
    [actions, api, dispatch],
  );

  return [state, controller];
};
