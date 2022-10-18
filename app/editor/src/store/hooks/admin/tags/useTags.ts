import { IPaged, ITagFilter, ITagModel, useApiAdminTags } from 'hooks';
import React from 'react';
import { useApiDispatcher } from 'store/hooks';
import { IAdminState, useAdminStore } from 'store/slices';

interface ITagController {
  findAllTags: () => Promise<ITagModel[]>;
  findTag: (filter: ITagFilter) => Promise<IPaged<ITagModel>>;
  getTag: (id: string) => Promise<ITagModel>;
  addTag: (model: ITagModel) => Promise<ITagModel>;
  updateTag: (model: ITagModel) => Promise<ITagModel>;
  deleteTag: (model: ITagModel) => Promise<ITagModel>;
}

export const useTags = (): [IAdminState, ITagController] => {
  const api = useApiAdminTags();
  const dispatch = useApiDispatcher();
  const [state, store] = useAdminStore();

  const controller = React.useMemo(
    () => ({
      findAllTags: async () => {
        const result = await dispatch<ITagModel[]>('find-all-tags', () => api.findAllTags());
        store.storeTags(result);
        return result;
      },
      findTag: async (filter: ITagFilter) => {
        const result = await dispatch<IPaged<ITagModel>>('find-tag', () => api.findTags(filter));
        return result;
      },
      getTag: async (id: string) => {
        const result = await dispatch<ITagModel>('get-tag', () => api.getTag(id));
        store.storeTags(
          state.tags.map((ds) => {
            if (ds.id === result.id) return result;
            return ds;
          }),
        );
        return result;
      },
      addTag: async (model: ITagModel) => {
        const result = await dispatch<ITagModel>('add-tag', () => api.addTag(model));
        store.storeTags([...state.tags, result]);
        return result;
      },
      updateTag: async (model: ITagModel) => {
        const result = await dispatch<ITagModel>('update-tag', () => api.updateTag(model));
        store.storeTags(
          state.tags.map((ds) => {
            if (ds.id === result.id) return result;
            return ds;
          }),
        );
        return result;
      },
      deleteTag: async (model: ITagModel) => {
        const result = await dispatch<ITagModel>('delete-tag', () => api.deleteTag(model));
        store.storeTags(state.tags.filter((ds) => ds.id !== result.id));
        return result;
      },
    }),
    // The state.mediaTypes will cause it to fire twice!
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [api, dispatch, store],
  );

  return [state, controller];
};
