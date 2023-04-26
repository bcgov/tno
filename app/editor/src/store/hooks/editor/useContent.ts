import {
  IContentListAdvancedFilter,
  IContentListFilter,
} from 'features/content/list-view/interfaces';
import { IMorningReportsFilter } from 'features/content/morning-papers/interfaces';
import React from 'react';
import { ActionDelegate } from 'store';
import { useContentStore } from 'store/slices';
import { IContentProps, IContentState } from 'store/slices/content';
import {
  ContentListActionName,
  IContentFilter,
  IContentListModel,
  IContentModel,
  IPaged,
  useApiContents,
} from 'tno-core';

import { useAjaxWrapper } from '..';

interface IContentController {
  findContent: (filter: IContentFilter) => Promise<IPaged<IContentModel>>;
  getContent: (id: number) => Promise<IContentModel | undefined>;
  addContent: (content: IContentModel) => Promise<IContentModel>;
  updateContent: (content: IContentModel) => Promise<IContentModel>;
  updateContentList: (content: IContentListModel) => Promise<IContentModel[]>;
  deleteContent: (content: IContentModel) => Promise<IContentModel>;
  publishContent: (content: IContentModel) => Promise<IContentModel>;
  unpublishContent: (content: IContentModel) => Promise<IContentModel>;
  upload: (content: IContentModel, file: File) => Promise<IContentModel>;
  download: (id: number, fileName: string) => Promise<unknown>;
  attach: (contentId: number, locationId: number, path: string) => Promise<IContentModel>;
  storeFilter: (filter: IContentListFilter | ActionDelegate<IContentListFilter>) => void;
  stream: (path: string) => Promise<string>;
  storeFilterAdvanced: (
    filter: IContentListAdvancedFilter | ActionDelegate<IContentListAdvancedFilter>,
  ) => void;
  storeFilterMorningReport: (
    filter: IMorningReportsFilter | ActionDelegate<IMorningReportsFilter>,
  ) => void;
  storeFilterMorningReportAdvanced: (
    filter: IContentListAdvancedFilter | ActionDelegate<IContentListAdvancedFilter>,
  ) => void;
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
        const response = await dispatch('get-content', () => api.getContent(id), 'content');
        return response.status === 204 ? undefined : response.data;
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
      updateContentList: async (content: IContentListModel) => {
        const response = await dispatch(
          'update-content-list',
          () => api.updateContentList(content),
          'content',
        );

        switch (content.action) {
          case ContentListActionName.Publish:
          case ContentListActionName.Unpublish:
          case ContentListActionName.Action:
            actions.updateContent(
              response.data.map((i) => {
                i.isSelected = true;
                return i;
              }),
            );
            break;
          case ContentListActionName.Hide:
          case ContentListActionName.Unhide:
            actions.removeContent(response.data);
            break;
        }
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
      attach: async (contentId: number, locationId: number, path: string) => {
        return (
          await dispatch<IContentModel>(
            'attach-content',
            () => api.attach(contentId, locationId, path),
            'content',
          )
        ).data;
      },
      stream: async (path: string) => {
        return (await dispatch('stream-content', () => api.stream(path), 'content')).data;
      },
      storeFilter: actions.storeFilter,
      storeFilterAdvanced: actions.storeFilterAdvanced,
      storeFilterMorningReport: actions.storeFilterMorningReport,
      storeFilterMorningReportAdvanced: actions.storeFilterMorningReportAdvanced,
    }),
    [actions, api, dispatch],
  );

  return [state, controller];
};
