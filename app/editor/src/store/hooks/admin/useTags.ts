import { IPaged, ITagFilter, ITagModel, useApiAdminTags } from 'hooks';
import React from 'react';
import { useAjaxWrapper, useLookup } from 'store/hooks';
import { IAdminState, useAdminStore } from 'store/slices';

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
  const [, lookup] = useLookup();

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
        store.storeTags((tags) =>
          tags.map((ds) => {
            if (ds.id === response.data.id) return response.data;
            return ds;
          }),
        );
        return response.data;
      },
      addTag: async (model: ITagModel) => {
        const response = await dispatch<ITagModel>('add-tag', () => api.addTag(model));
        store.storeTags((tags) => [...tags, response.data]);
        await lookup.getLookups();
        return response.data;
      },
      updateTag: async (model: ITagModel) => {
        const response = await dispatch<ITagModel>('update-tag', () => api.updateTag(model));
        store.storeTags((tags) =>
          tags.map((ds) => {
            if (ds.id === response.data.id) return response.data;
            return ds;
          }),
        );
        await lookup.getLookups();
        return response.data;
      },
      deleteTag: async (model: ITagModel) => {
        const response = await dispatch<ITagModel>('delete-tag', () => api.deleteTag(model));
        store.storeTags((tags) => tags.filter((ds) => ds.id !== response.data.id));
        await lookup.getLookups();
        return response.data;
      },
    }),
    [api, dispatch, lookup, store],
  );

  return [state, controller];
};
