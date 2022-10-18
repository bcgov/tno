import {
  IIngestTypeFilter,
  IIngestTypeModel,
  IPaged,
  useApiAdminIngestTypes,
} from 'hooks/api-editor';
import React from 'react';
import { useApiDispatcher } from 'store/hooks';
import { IAdminState, useAdminStore } from 'store/slices';

interface IIngestTypeController {
  findAllIngestTypes: () => Promise<IIngestTypeModel[]>;
  findIngestTypes: (filter: IIngestTypeFilter) => Promise<IPaged<IIngestTypeModel>>;
  getIngestType: (id: number) => Promise<IIngestTypeModel>;
  addIngestType: (model: IIngestTypeModel) => Promise<IIngestTypeModel>;
  updateIngestType: (model: IIngestTypeModel) => Promise<IIngestTypeModel>;
  deleteIngestType: (model: IIngestTypeModel) => Promise<IIngestTypeModel>;
}

export const useIngestTypes = (): [IAdminState, IIngestTypeController] => {
  const api = useApiAdminIngestTypes();
  const dispatch = useApiDispatcher();
  const [state, store] = useAdminStore();

  const controller = React.useMemo(
    () => ({
      findAllIngestTypes: async () => {
        const response = await dispatch<IIngestTypeModel[]>('find-all-ingest-types', () =>
          api.findAllIngestTypes(),
        );
        store.storeIngestTypes(response.data);
        return response.data;
      },
      findIngestTypes: async (filter: IIngestTypeFilter) => {
        const response = await dispatch<IPaged<IIngestTypeModel>>('find-ingest-types', () =>
          api.findIngestTypes(filter),
        );
        return response.data;
      },
      getIngestType: async (id: number) => {
        const response = await dispatch<IIngestTypeModel>('get-ingest-type', () =>
          api.getIngestType(id),
        );
        store.storeIngestTypes(
          state.ingestTypes.map((ds) => {
            if (ds.id === response.data.id) return response.data;
            return ds;
          }),
        );
        return response.data;
      },
      addIngestType: async (model: IIngestTypeModel) => {
        const response = await dispatch<IIngestTypeModel>('add-ingest-type', () =>
          api.addIngestType(model),
        );
        store.storeIngestTypes([...state.ingestTypes, response.data]);
        return response.data;
      },
      updateIngestType: async (model: IIngestTypeModel) => {
        const response = await dispatch<IIngestTypeModel>('update-ingest-type', () =>
          api.updateIngestType(model),
        );
        store.storeIngestTypes(
          state.ingestTypes.map((ds) => {
            if (ds.id === response.data.id) return response.data;
            return ds;
          }),
        );
        return response.data;
      },
      deleteIngestType: async (model: IIngestTypeModel) => {
        const response = await dispatch<IIngestTypeModel>('delete-ingest-type', () =>
          api.deleteIngestType(model),
        );
        store.storeIngestTypes(state.ingestTypes.filter((ds) => ds.id !== response.data.id));
        return response.data;
      },
    }),
    // The state.ingestTypes will cause it to fire twice!
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [api, dispatch, store],
  );

  return [state, controller];
};
