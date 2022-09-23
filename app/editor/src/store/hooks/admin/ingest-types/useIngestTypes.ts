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
        const result = await dispatch<IIngestTypeModel[]>('find-all-ingest-types', () =>
          api.findAllIngestTypes(),
        );
        store.storeIngestTypes(result);
        return result;
      },
      findIngestTypes: async (filter: IIngestTypeFilter) => {
        const result = await dispatch<IPaged<IIngestTypeModel>>('find-ingest-types', () =>
          api.findIngestTypes(filter),
        );
        return result;
      },
      getIngestType: async (id: number) => {
        const result = await dispatch<IIngestTypeModel>('get-ingest-type', () =>
          api.getIngestType(id),
        );
        store.storeIngestTypes(
          state.ingestTypes.map((ds) => {
            if (ds.id === result.id) return result;
            return ds;
          }),
        );
        return result;
      },
      addIngestType: async (model: IIngestTypeModel) => {
        const result = await dispatch<IIngestTypeModel>('add-ingest-type', () =>
          api.addIngestType(model),
        );
        store.storeIngestTypes([...state.ingestTypes, result]);
        return result;
      },
      updateIngestType: async (model: IIngestTypeModel) => {
        const result = await dispatch<IIngestTypeModel>('update-ingest-type', () =>
          api.updateIngestType(model),
        );
        store.storeIngestTypes(
          state.ingestTypes.map((ds) => {
            if (ds.id === result.id) return result;
            return ds;
          }),
        );
        return result;
      },
      deleteIngestType: async (model: IIngestTypeModel) => {
        const result = await dispatch<IIngestTypeModel>('delete-ingest-type', () =>
          api.deleteIngestType(model),
        );
        store.storeIngestTypes(state.ingestTypes.filter((ds) => ds.id !== result.id));
        return result;
      },
    }),
    // The state.ingestTypes will cause it to fire twice!
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [api, dispatch, store],
  );

  return [state, controller];
};
