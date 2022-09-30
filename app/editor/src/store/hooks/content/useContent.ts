import {
  IContentListAdvancedFilter,
  IContentListFilter,
} from 'features/content/list-view/interfaces';
import { IContentFilter, IContentModel, IPaged } from 'hooks/api-editor';
import { useApiContents } from 'hooks/api-editor';
import React from 'react';
import { useContentStore } from 'store/slices';
import { IContentProps, IContentState } from 'store/slices/content';

import { useApiDispatcher } from '..';

interface IContentController {
  getContent: (id: number) => Promise<IContentModel>;
  findContent: (filter: IContentFilter) => Promise<IPaged<IContentModel>>;
  addContent: (content: IContentModel) => Promise<IContentModel>;
  updateContent: (content: IContentModel) => Promise<IContentModel>;
  deleteContent: (content: IContentModel) => Promise<IContentModel>;
  transcribe: (content: IContentModel) => Promise<IContentModel>;
  nlp: (content: IContentModel) => Promise<IContentModel>;
  publishContent: (content: IContentModel) => Promise<IContentModel>;
  unpublishContent: (content: IContentModel) => Promise<IContentModel>;
  upload: (content: IContentModel, file: File) => Promise<IContentModel>;
  download: (id: number, fileName: string) => Promise<unknown>;
  attach: (id: number, path: string) => Promise<IContentModel>;
  storeFilter: (filter: IContentListFilter) => void;
  storeFilterAdvanced: (filter: IContentListAdvancedFilter) => void;
}

export const useContent = (props?: IContentProps): [IContentState, IContentController] => {
  const [state, actions] = useContentStore(props);
  const dispatch = useApiDispatcher();
  const api = useApiContents();

  const controller = React.useRef({
    getContent: async (id: number) => {
      return await dispatch('get-content', () => api.getContent(id));
    },
    findContent: async (filter: IContentFilter) => {
      const result = await dispatch('find-contents', () => api.findContent(filter));
      actions.storeContent(result);
      return result;
    },
    addContent: async (content: IContentModel) => {
      return await dispatch('add-content', () => api.addContent(content));
    },
    updateContent: async (content: IContentModel) => {
      const result = await dispatch('update-content', () => api.updateContent(content));
      if (state.content) {
        actions.storeContent({
          ...state.content,
          items: state.content.items.map((i: IContentModel) => {
            if (i.id === content.id) return content;
            return i;
          }),
        });
      }
      return result;
    },
    deleteContent: async (content: IContentModel) => {
      return await dispatch('delete-content', () => api.deleteContent(content));
    },
    transcribe: async (content: IContentModel) => {
      return await dispatch('transcribe-content', () => api.transcribe(content));
    },
    nlp: async (content: IContentModel) => {
      return await dispatch('nlp-content', () => api.nlp(content));
    },
    publishContent: async (content: IContentModel) => {
      return await dispatch('publish-content', () => api.publishContent(content));
    },
    unpublishContent: async (content: IContentModel) => {
      return await dispatch('unpublish-content', () => api.unpublishContent(content));
    },
    upload: async (content: IContentModel, file: File) => {
      return await dispatch('upload-content', () => api.upload(content, file));
    },
    download: async (id: number, fileName: string) => {
      return await dispatch('download-content', () => api.download(id, fileName));
    },
    attach: async (id: number, path: string) => {
      return await dispatch<IContentModel>('attach-content', () => api.attach(id, path));
    },
    storeFilter: actions.storeFilter,
    storeFilterAdvanced: actions.storeFilterAdvanced,
  }).current;

  return [state, controller];
};
