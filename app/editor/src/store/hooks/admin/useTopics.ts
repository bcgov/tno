import React from 'react';
import { useAjaxWrapper, useLookup } from 'store/hooks';
import { IAdminState, useAdminStore } from 'store/slices';
import {
  IPaged,
  ITopicFilter,
  ITopicModel,
  saveToLocalStorage,
  StorageKeys,
  useApiAdminTopics,
} from 'tno-core';

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
  const [state, { storeTopics }] = useAdminStore();
  const [, lookup] = useLookup();

  const controller = React.useMemo(
    () => ({
      findAllTopics: async () => {
        const response = await dispatch<ITopicModel[]>('find-all-topics', () =>
          api.findAllTopics(),
        );
        storeTopics(response.data);
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
        storeTopics((topics) =>
          topics.map((t) => {
            if (t.id === response.data.id) return response.data;
            return t;
          }),
        );
        return response.data;
      },
      addTopic: async (model: ITopicModel) => {
        const response = await dispatch<ITopicModel>('add-topic', () => api.addTopic(model));
        let items: ITopicModel[] = [];

        storeTopics((topics) => {
          items = [...topics];
          items.splice(model.sortOrder, 0, response.data);
          return items;
        });
        saveToLocalStorage(StorageKeys.Topics, items);
        return response.data;
      },
      updateTopic: async (model: ITopicModel) => {
        const response = await dispatch<ITopicModel>('update-topic', () => api.updateTopic(model));
        storeTopics((topics) =>
          topics.map((t) => {
            if (t.id === response.data.id) return response.data;
            return t;
          }),
        );
        await lookup.getLookups();
        return response.data;
      },
      deleteTopic: async (model: ITopicModel) => {
        const response = await dispatch<ITopicModel>('delete-topic', () => api.deleteTopic(model));
        storeTopics((topics) => topics.filter((t) => t.id !== response.data.id));
        await lookup.getLookups();
        return response.data;
      },
    }),
    [api, dispatch, lookup, storeTopics],
  );

  return [state, controller];
};
