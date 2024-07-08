import React from 'react';
import { useAjaxWrapper, useLookup } from 'store/hooks';
import { IAdminState, useAdminStore } from 'store/slices';
import { IFilterFilter, IFilterModel, useApiAdminFilters } from 'tno-core';

interface IFilterController {
  findFilters: (filter: IFilterFilter) => Promise<IFilterModel[]>;
  getFilter: (id: number) => Promise<IFilterModel>;
  addFilter: (model: IFilterModel) => Promise<IFilterModel>;
  updateFilter: (model: IFilterModel) => Promise<IFilterModel>;
  deleteFilter: (model: IFilterModel) => Promise<IFilterModel>;
}

export const useFilters = (): [IAdminState & { initialized: boolean }, IFilterController] => {
  const api = useApiAdminFilters();
  const dispatch = useAjaxWrapper();
  const [state, store] = useAdminStore();
  const [, lookup] = useLookup();
  const [initialized, setInitialized] = React.useState(false);

  const controller = React.useMemo(
    () => ({
      findFilters: async (filter: IFilterFilter) => {
        const response = await dispatch<IFilterModel[]>('find-all-filters', () =>
          api.findFilters(filter),
        );
        store.storeFilters(response.data);
        setInitialized(true);
        return response.data;
      },
      getFilter: async (id: number) => {
        const response = await dispatch<IFilterModel>('get-filter', () => api.getFilter(id));
        store.storeFilters((filters) =>
          filters.map((ds) => {
            if (ds.id === response.data.id) return response.data;
            return ds;
          }),
        );
        return response.data;
      },
      addFilter: async (model: IFilterModel) => {
        const response = await dispatch<IFilterModel>('add-filter', () => api.addFilter(model));
        store.storeFilters((filters) => [...filters, response.data]);
        await lookup.getLookups();
        return response.data;
      },
      updateFilter: async (model: IFilterModel) => {
        const response = await dispatch<IFilterModel>('update-filter', () =>
          api.updateFilter(model),
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
        const response = await dispatch<IFilterModel>('delete-filter', () =>
          api.deleteFilter(model),
        );
        store.storeFilters((filters) => filters.filter((ds) => ds.id !== response.data.id));
        await lookup.getLookups();
        return response.data;
      },
    }),
    [api, dispatch, lookup, store],
  );

  return [{ ...state, initialized }, controller];
};
