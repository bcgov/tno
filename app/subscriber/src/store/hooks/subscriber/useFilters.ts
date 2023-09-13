import React from 'react';
import { useAjaxWrapper } from 'store/hooks';
import { IProfileState, useProfileStore } from 'store/slices';
import { IFilterModel, useApiSubscriberFilters } from 'tno-core';

interface IFilterController {
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
        const response = await dispatch<IFilterModel[]>('find-my-folders', () =>
          api.findMyFilters(),
        );
        store.storeMyFilters(response.data);
        return response.data;
      },
      getFilter: async (id: number) => {
        const response = await dispatch<IFilterModel>('get-folder', () => api.getFilter(id));
        store.storeMyFilters((folders) =>
          folders.map((ds) => {
            if (ds.id === response.data.id) return response.data;
            return ds;
          }),
        );
        return response.data;
      },
      addFilter: async (model: IFilterModel) => {
        const response = await dispatch<IFilterModel>('add-folder', () => api.addFilter(model));
        store.storeMyFilters((folders) => [...folders, response.data]);
        return response.data;
      },
      updateFilter: async (model: IFilterModel) => {
        const response = await dispatch<IFilterModel>('update-folder', () =>
          api.updateFilter(model),
        );
        store.storeMyFilters((folders) =>
          folders.map((ds) => {
            if (ds.id === response.data.id) return response.data;
            return ds;
          }),
        );
        return response.data;
      },
      deleteFilter: async (model: IFilterModel) => {
        const response = await dispatch<IFilterModel>('delete-folder', () =>
          api.deleteFilter(model),
        );
        store.storeMyFilters((folders) => folders.filter((ds) => ds.id !== response.data.id));
        return response.data;
      },
    }),
    [api, dispatch, store],
  );

  return [state, controller];
};
