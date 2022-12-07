import {
  IContentListAdvancedFilter,
  IContentListFilter,
} from 'features/content/list-view/interfaces';
import { IMorningReportFilter } from 'features/content/morning-report/interfaces';
import { IContentFilter, IContentModel, IPaged } from 'hooks/api-editor';
import { useApiContents } from 'hooks/api-editor';
import React from 'react';
import { useContentStore } from 'store/slices';
import { IContentProps, IContentState } from 'store/slices/content';

import { useAjaxWrapper } from '..';

interface IContentController {
  findContent: (filter: IContentFilter) => Promise<IPaged<IContentModel>>;
  getContent: (id: number) => Promise<IContentModel>;
  addContent: (content: IContentModel) => Promise<IContentModel>;
  updateContent: (content: IContentModel) => Promise<IContentModel>;
  deleteContent: (content: IContentModel) => Promise<IContentModel>;
  publishContent: (content: IContentModel) => Promise<IContentModel>;
  unpublishContent: (content: IContentModel) => Promise<IContentModel>;
  upload: (content: IContentModel, file: File) => Promise<IContentModel>;
  download: (id: number, fileName: string) => Promise<unknown>;
  attach: (id: number, path: string) => Promise<IContentModel>;
  storeFilter: (filter: IContentListFilter) => void;
  storeFilterAdvanced: (filter: IContentListAdvancedFilter) => void;
  storeMorningReportFilter: (filter: IMorningReportFilter) => void;
}

export const useContent = (props?: IContentProps): [IContentState, IContentController] => {
  const [state, actions] = useContentStore(props);
  const dispatch = useAjaxWrapper();
  const api = useApiContents();

  const controller = React.useMemo(
    () => ({
      findContent: async (filter: IContentFilter) => {
        const response = await dispatch('find-contents', () => api.findContent(filter));
        actions.storeContent(response.data);
        return response.data;
      },
      getContent: async (id: number) => {
        return (await dispatch('get-content', () => api.getContent(id), 'content')).data;
      },
      addContent: async (content: IContentModel) => {
        const response = await dispatch('add-content', () => api.addContent(content), 'content');
        actions.addContent([response.data]);
        return response.data;
      },
      updateContent: async (content: IContentModel) => {
        const response = await dispatch(
          'update-content',
          () => api.updateContent(content),
          'content',
        );
        actions.updateContent([response.data]);
        return response.data;
      },
      deleteContent: async (content: IContentModel) => {
        const response = await dispatch(
          'delete-content',
          () => api.deleteContent(content),
          'content',
        );
        actions.removeContent([response.data]);
        return response.data;
      },
      publishContent: async (content: IContentModel) => {
        return (await dispatch('publish-content', () => api.publishContent(content), 'content'))
          .data;
      },
      unpublishContent: async (content: IContentModel) => {
        return (await dispatch('unpublish-content', () => api.unpublishContent(content), 'content'))
          .data;
      },
      upload: async (content: IContentModel, file: File) => {
        return (await dispatch('upload-content', () => api.upload(content, file), 'content')).data;
      },
      download: async (id: number, fileName: string) => {
        return (await dispatch('download-content', () => api.download(id, fileName), 'content'))
          .data;
      },
      attach: async (id: number, path: string) => {
        return (
          await dispatch<IContentModel>('attach-content', () => api.attach(id, path), 'content')
        ).data;
      },
      storeFilter: actions.storeFilter,
      storeFilterAdvanced: actions.storeFilterAdvanced,
      storeMorningReportFilter: actions.storeMorningReportFilter,
    }),
    [actions, api, dispatch],
  );

  return [state, controller];
};
