import React from 'react';
import { useAjaxWrapper } from 'store/hooks';
import { IAdminState, useAdminStore } from 'store/slices';
import { IPaged, ITagFilter, ITagModel, useApiAdminTags } from 'tno-core';

interface ITagController {
  findAllTags: () => Promise<ITagModel[]>;
  findTag: (filter: ITagFilter) => Promise<IPaged<ITagModel>>;
  getTag: (id: number) => Promise<ITagModel>;
  addTag: (model: ITagModel) => Promise<ITagModel>;
  updateTag: (model: ITagModel) => Promise<ITagModel>;
  deleteTag: (model: ITagModel) => Promise<ITagModel>;
}

export const useTags = (): [IAdminState, ITagController] => {
  const api = useApiAdminTags();
  const dispatch = useAjaxWrapper();
  const [state, store] = useAdminStore();

  const controller = React.useMemo(
    () => ({
      findAllTags: async () => {
        const response = await dispatch<ITagModel[]>('find-all-tags', () => api.findAllTags());
        store.storeTags(response.data);
        return response.data;
      },
      findTag: async (filter: ITagFilter) => {
        const response = await dispatch<IPaged<ITagModel>>('find-tag', () => api.findTags(filter));
        return response.data;
      },
      getTag: async (id: number) => {
        const response = await dispatch<ITagModel>('get-tag', () => api.getTag(id));
        store.storeTags(
          state.tags.map((ds) => {
            if (ds.id === response.data.id) return response.data;
            return ds;
          }),
        );
        return response.data;
      },
      addTag: async (model: ITagModel) => {
        const response = await dispatch<ITagModel>('add-tag', () => api.addTag(model));
        store.storeTags([...state.tags, response.data]);
        return response.data;
      },
      updateTag: async (model: ITagModel) => {
        const response = await dispatch<ITagModel>('update-tag', () => api.updateTag(model));
        store.storeTags(
          state.tags.map((ds) => {
            if (ds.id === response.data.id) return response.data;
            return ds;
          }),
        );
        return response.data;
      },
      deleteTag: async (model: ITagModel) => {
        const response = await dispatch<ITagModel>('delete-tag', () => api.deleteTag(model));
        store.storeTags(state.tags.filter((ds) => ds.id !== response.data.id));
        return response.data;
      },
    }),
    // The state.mediaTypes will cause it to fire twice!
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [api, dispatch, store],
  );

  return [state, controller];
};
