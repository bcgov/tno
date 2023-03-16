import { IDataLocationModel, useApiAdminDataLocations } from 'hooks/api-editor';
import React from 'react';
import { useAjaxWrapper, useLookup } from 'store/hooks';
import { IAdminState, useAdminStore } from 'store/slices';

interface IDataLocationController {
  findAllDataLocations: () => Promise<IDataLocationModel[]>;
  getDataLocation: (id: number) => Promise<IDataLocationModel>;
  addDataLocation: (model: IDataLocationModel) => Promise<IDataLocationModel>;
  updateDataLocation: (model: IDataLocationModel) => Promise<IDataLocationModel>;
  deleteDataLocation: (model: IDataLocationModel) => Promise<IDataLocationModel>;
}

export const useDataLocations = (): [IAdminState, IDataLocationController] => {
  const api = useApiAdminDataLocations();
  const dispatch = useAjaxWrapper();
  const [state, store] = useAdminStore();
  const [, lookup] = useLookup();

  const controller = React.useMemo(
    () => ({
      findAllDataLocations: async () => {
        const response = await dispatch<IDataLocationModel[]>('find-all-data-locations', () =>
          api.findAllDataLocations(),
        );
        store.storeDataLocations(response.data);
        return response.data;
      },
      getDataLocation: async (id: number) => {
        const response = await dispatch<IDataLocationModel>('get-data-location', () =>
          api.getDataLocation(id),
        );
        store.storeDataLocations((dataLocations) =>
          dataLocations.map((ds) => {
            if (ds.id === response.data.id) return response.data;
            return ds;
          }),
        );
        return response.data;
      },
      addDataLocation: async (model: IDataLocationModel) => {
        const response = await dispatch<IDataLocationModel>('add-data-location', () =>
          api.addDataLocation(model),
        );
        store.storeDataLocations((dataLocations) => [...dataLocations, response.data]);
        await lookup.getLookups();
        return response.data;
      },
      updateDataLocation: async (model: IDataLocationModel) => {
        const response = await dispatch<IDataLocationModel>('update-data-location', () =>
          api.updateDataLocation(model),
        );
        store.storeDataLocations((dataLocations) =>
          dataLocations.map((ds) => {
            if (ds.id === response.data.id) return response.data;
            return ds;
          }),
        );
        await lookup.getLookups();
        return response.data;
      },
      deleteDataLocation: async (model: IDataLocationModel) => {
        const response = await dispatch<IDataLocationModel>('delete-data-location', () =>
          api.deleteDataLocation(model),
        );
        store.storeDataLocations((dataLocations) =>
          dataLocations.filter((ds) => ds.id !== response.data.id),
        );
        await lookup.getLookups();
        return response.data;
      },
    }),
    [dispatch, store, api, lookup],
  );

  return [state, controller];
};
