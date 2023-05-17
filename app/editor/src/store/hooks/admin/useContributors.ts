import React from 'react';
import { useAjaxWrapper, useLookup } from 'store/hooks';
import { IAdminState, useAdminStore } from 'store/slices';
import { IContributorModel, useApiAdminContributors } from 'tno-core';

interface IContributorController {
  findAllContributor: () => Promise<IContributorModel[]>;
  getContributor: (id: number) => Promise<IContributorModel>;
  addContributor: (model: IContributorModel) => Promise<IContributorModel>;
  updateContributor: (model: IContributorModel) => Promise<IContributorModel>;
  deleteContributor: (model: IContributorModel) => Promise<IContributorModel>;
}

export const useContributors = (): [IAdminState, IContributorController] => {
  const api = useApiAdminContributors();
  const dispatch = useAjaxWrapper();
  const [state, store] = useAdminStore();
  const [, lookup] = useLookup();

  const controller = React.useMemo(
    () => ({
      findAllContributor: async () => {
        const response = await dispatch<IContributorModel[]>('find-all-contributors', () =>
          api.findAllContributors(),
        );
        store.storeContributors(response.data);
        return response.data;
      },
      getContributor: async (id: number) => {
        const response = await dispatch<IContributorModel>('get-contributors', () =>
          api.getContributor(id),
        );
        store.storeContributors((contributors) =>
          contributors.map((ds) => {
            if (ds.id === response.data.id) return response.data;
            return ds;
          }),
        );
        return response.data;
      },
      addContributor: async (model: IContributorModel) => {
        const response = await dispatch<IContributorModel>('add-contributors', () =>
          api.addContributor(model),
        );
        store.storeContributors((contributors) => [...contributors, response.data]);
        await lookup.getLookups();
        return response.data;
      },
      updateContributor: async (model: IContributorModel) => {
        const response = await dispatch<IContributorModel>('update-contributors', () =>
          api.updateContributor(model),
        );
        store.storeContributors((contributors) =>
          contributors.map((ds) => {
            if (ds.id === response.data.id) return response.data;
            return ds;
          }),
        );
        await lookup.getLookups();
        return response.data;
      },
      deleteContributor: async (model: IContributorModel) => {
        const response = await dispatch<IContributorModel>('delete-contributors', () =>
          api.deleteContributor(model),
        );
        store.storeContributors((contributors) =>
          contributors.filter((ds) => ds.id !== response.data.id),
        );
        await lookup.getLookups();
        return response.data;
      },
    }),
    [api, dispatch, lookup, store],
  );

  return [state, controller];
};
