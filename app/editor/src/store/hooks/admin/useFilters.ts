import React from 'react';
import { useAjaxWrapper, useLookup } from 'store/hooks';
import { type IAdminState, useAdminStore } from 'store/slices';
import {
  type IFilterAIChatRequestModel,
  type IFilterAIChatResultModel,
  type IFilterAIGenerateRequestModel,
  type IFilterAIGenerateResultModel,
  type IFilterFilter,
  type IFilterModel,
  useApiAdminFilters,
} from 'tno-core';

interface IFilterController {
  findFilters: (filter: IFilterFilter, isSilent?: boolean) => Promise<IFilterModel[]>;
  getFilter: (id: number) => Promise<IFilterModel>;
  addFilter: (model: IFilterModel) => Promise<IFilterModel>;
  updateFilter: (model: IFilterModel) => Promise<IFilterModel>;
  deleteFilter: (model: IFilterModel) => Promise<IFilterModel>;
  generateFilter: (model: IFilterAIGenerateRequestModel) => Promise<IFilterAIGenerateResultModel>;
  chatFilter: (model: IFilterAIChatRequestModel) => Promise<IFilterAIChatResultModel>;
}

export const useFilters = (): [IAdminState & { initialized: boolean }, IFilterController] => {
  const api = useApiAdminFilters();
  const dispatch = useAjaxWrapper();
  const [state, store] = useAdminStore();
  const [, lookup] = useLookup();
  const [initialized, setInitialized] = React.useState(false);

  const controller = React.useMemo(
    () => ({
      findFilters: async (filter: IFilterFilter, isSilent?: boolean) => {
        const response = await dispatch<IFilterModel[]>(
          'find-all-filters',
          async () => await api.findFilters(filter),
          undefined,
          isSilent,
        );
        store.storeFilters(response.data);
        setInitialized(true);
        return response.data;
      },
      getFilter: async (id: number) => {
        const response = await dispatch<IFilterModel>(
          'get-filter',
          async () => await api.getFilter(id),
        );
        store.storeFilters((filters) =>
          filters.map((ds) => {
            if (ds.id === response.data.id) return response.data;
            return ds;
          }),
        );
        return response.data;
      },
      addFilter: async (model: IFilterModel) => {
        const response = await dispatch<IFilterModel>(
          'add-filter',
          async () => await api.addFilter(model),
        );
        store.storeFilters((filters) => [...filters, response.data]);
        await lookup.getLookups();
        return response.data;
      },
      updateFilter: async (model: IFilterModel) => {
        const response = await dispatch<IFilterModel>(
          'update-filter',
          async () => await api.updateFilter(model),
        );
        store.storeFilters((filters) =>
          filters.map((ds) => {
            if (ds.id === response.data.id) return response.data;
            return ds;
          }),
        );
        await lookup.getLookups();
        return response.data;
      },
      deleteFilter: async (model: IFilterModel) => {
        const response = await dispatch<IFilterModel>(
          'delete-filter',
          async () => await api.deleteFilter(model),
        );
        store.storeFilters((filters) => filters.filter((ds) => ds.id !== response.data.id));
        await lookup.getLookups();
        return response.data;
      },
      // The prompt shows its own progress, so these do not block the page.
      generateFilter: async (model: IFilterAIGenerateRequestModel) => {
        const response = await dispatch<IFilterAIGenerateResultModel>(
          'generate-filter',
          async () => await api.generateFilter(model),
          undefined,
          true,
        );
        return response.data;
      },
      chatFilter: async (model: IFilterAIChatRequestModel) => {
        const response = await dispatch<IFilterAIChatResultModel>(
          'chat-filter',
          async () => await api.chatFilter(model),
          undefined,
          true,
        );
        return response.data;
      },
    }),
    [api, dispatch, lookup, store],
  );

  return [{ ...state, initialized }, controller];
};
