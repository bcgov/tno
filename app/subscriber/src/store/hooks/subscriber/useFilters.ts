import React from 'react';
import { useAjaxWrapper } from 'store/hooks';
import { IProfileState, useProfileStore } from 'store/slices';
import { IFilterModel, useApiSubscriberFilters } from 'tno-core';

interface IFilterController {
  activeFilter?: IFilterModel;
  findMyFilters: () => Promise<IFilterModel[]>;
  getFilter: (id: number) => Promise<IFilterModel>;
  addFilter: (model: IFilterModel) => Promise<IFilterModel>;
  updateFilter: (model: IFilterModel) => Promise<IFilterModel>;
  deleteFilter: (model: IFilterModel) => Promise<IFilterModel>;
}

export const useFilters = (): [IProfileState, IFilterController] => {
  const api = useApiSubscriberFilters();
  const dispatch = useAjaxWrapper();
  const [state, store] = useProfileStore();

  const controller = React.useMemo(
    () => ({
      findMyFilters: async () => {
        const response = await dispatch<IFilterModel[]>('find-my-filter', () =>
          api.findMyFilters(),
        );
        store.storeMyFilters(response.data);
        return response.data;
      },
      getFilter: async (id: number) => {
        const response = await dispatch<IFilterModel>('get-filter', () => api.getFilter(id));
        const filter = response.data;
        store.storeMyFilters((filters) =>
          filters.map((ds) => {
            if (ds.id === filter.id) return filter;
            return ds;
          }),
        );
        store.storeFilter((state) => (state?.id === id ? filter : state));
        return filter;
      },
      addFilter: async (model: IFilterModel) => {
        const response = await dispatch<IFilterModel>('add-filter', () => api.addFilter(model));
        store.storeMyFilters((filters) => [...filters, response.data]);
        store.storeFilter((state) => (state?.id === model.id ? response.data : state));
        return response.data;
      },
      updateFilter: async (model: IFilterModel) => {
        const response = await dispatch<IFilterModel>('update-filter', () =>
          api.updateFilter(model),
        );
        store.storeMyFilters((filters) =>
          filters.map((ds) => {
            if (ds.id === response.data.id) return response.data;
            return ds;
          }),
        );
        store.storeFilter((state) => (state?.id === model.id ? response.data : state));
        return response.data;
      },
      deleteFilter: async (model: IFilterModel) => {
        const response = await dispatch<IFilterModel>('delete-filter', () =>
          api.deleteFilter(model),
        );
        store.storeMyFilters((filters) => filters.filter((ds) => ds.id !== response.data.id));
        store.storeFilter((state) => (state?.id === model.id ? undefined : state));
        return response.data;
      },
    }),
    [api, dispatch, store],
  );

  return [state, controller];
};
