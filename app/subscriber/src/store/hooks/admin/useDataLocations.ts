import { IDataLocationModel, useApiAdminDataLocations } from 'hooks/api-editor';
import React from 'react';
import { useAjaxWrapper } from 'store/hooks';
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
        store.storeDataLocations(
          state.dataLocations.map((ds) => {
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
        store.storeDataLocations([...state.dataLocations, response.data]);
        return response.data;
      },
      updateDataLocation: async (model: IDataLocationModel) => {
        const response = await dispatch<IDataLocationModel>('update-data-location', () =>
          api.updateDataLocation(model),
        );
        store.storeDataLocations(
          state.dataLocations.map((ds) => {
            if (ds.id === response.data.id) return response.data;
            return ds;
          }),
        );
        return response.data;
      },
      deleteDataLocation: async (model: IDataLocationModel) => {
        const response = await dispatch<IDataLocationModel>('delete-data-location', () =>
          api.deleteDataLocation(model),
        );
        store.storeDataLocations(state.dataLocations.filter((ds) => ds.id !== response.data.id));
        return response.data;
      },
    }),
    // The state.dataLocations will cause it to fire twice!
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [dispatch, store, api],
  );

  return [state, controller];
};
