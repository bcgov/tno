import React from 'react';
import { useAjaxWrapper } from 'store/hooks';
import { IAdminState, useAdminStore } from 'store/slices';
import { IPaged, ITopicFilter, ITopicModel, useApiAdminTopics } from 'tno-core';

interface ITopicController {
  findAllTopics: () => Promise<ITopicModel[]>;
  findTopic: (filter: ITopicFilter) => Promise<IPaged<ITopicModel>>;
  getTopic: (id: number) => Promise<ITopicModel>;
  addTopic: (model: ITopicModel) => Promise<ITopicModel>;
  updateTopic: (model: ITopicModel) => Promise<ITopicModel>;
  deleteTopic: (model: ITopicModel) => Promise<ITopicModel>;
}

export const useTopics = (): [IAdminState, ITopicController] => {
  const api = useApiAdminTopics();
  const dispatch = useAjaxWrapper();
  const [state, store] = useAdminStore();

  const controller = React.useMemo(
    () => ({
      findAllTopics: async () => {
        const response = await dispatch<ITopicModel[]>('find-all-topics', () =>
          api.findAllTopics(),
        );
        store.storeTopics(response.data);
        return response.data;
      },
      findTopic: async (filter: ITopicFilter) => {
        const response = await dispatch<IPaged<ITopicModel>>('find-topics', () =>
          api.findTopics(filter),
        );
        return response.data;
      },
      getTopic: async (id: number) => {
        const response = await dispatch<ITopicModel>('get-topic', () => api.getTopic(id));
        store.storeTopics(
          state.topics.map((t) => {
            if (t.id === response.data.id) return response.data;
            return t;
          }),
        );
        return response.data;
      },
      addTopic: async (model: ITopicModel) => {
        const response = await dispatch<ITopicModel>('add-topic', () => api.addTopic(model));
        var items = [...state.topics];
        items.splice(model.sortOrder, 0, response.data);
        store.storeTopics(items);
        return response.data;
      },
      updateTopic: async (model: ITopicModel) => {
        const response = await dispatch<ITopicModel>('update-topic', () => api.updateTopic(model));
        store.storeTopics(
          state.topics.map((t) => {
            if (t.id === response.data.id) return response.data;
            return t;
          }),
        );
        return response.data;
      },
      deleteTopic: async (model: ITopicModel) => {
        const response = await dispatch<ITopicModel>('delete-topic', () => api.deleteTopic(model));
        store.storeTopics(state.topics.filter((t) => t.id !== response.data.id));
        return response.data;
      },
    }),
    [api, dispatch, state.topics, store],
  );

  return [state, controller];
};
