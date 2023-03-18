import React from 'react';
import { useAjaxWrapper, useLookup } from 'store/hooks';
import { IAdminState, useAdminStore } from 'store/slices';
import { IIngestTypeFilter, IIngestTypeModel, IPaged, useApiAdminIngestTypes } from 'tno-core';

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
  const dispatch = useAjaxWrapper();
  const [state, store] = useAdminStore();
  const [, lookup] = useLookup();

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
        store.storeIngestTypes((ingestTypes) =>
          ingestTypes.map((ds) => {
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
        store.storeIngestTypes((ingestTypes) => [...ingestTypes, response.data]);
        await lookup.getLookups();
        return response.data;
      },
      updateIngestType: async (model: IIngestTypeModel) => {
        const response = await dispatch<IIngestTypeModel>('update-ingest-type', () =>
          api.updateIngestType(model),
        );
        store.storeIngestTypes((ingestTypes) =>
          ingestTypes.map((ds) => {
            if (ds.id === response.data.id) return response.data;
            return ds;
          }),
        );
        await lookup.getLookups();
        return response.data;
      },
      deleteIngestType: async (model: IIngestTypeModel) => {
        const response = await dispatch<IIngestTypeModel>('delete-ingest-type', () =>
          api.deleteIngestType(model),
        );
        store.storeIngestTypes((ingestTypes) =>
          ingestTypes.filter((ds) => ds.id !== response.data.id),
        );
        await lookup.getLookups();
        return response.data;
      },
    }),
    [api, dispatch, lookup, store],
  );

  return [state, controller];
};
