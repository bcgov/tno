import React from 'react';
import { useAjaxWrapper, useLookup } from 'store/hooks';
import { IAdminState, useAdminStore } from 'store/slices';
import { IOrganizationModel, useApiAdminOrganizations } from 'tno-core';

interface IOrganizationController {
  findAllOrganizations: () => Promise<IOrganizationModel[]>;
  getOrganization: (id: number) => Promise<IOrganizationModel>;
  addOrganization: (model: IOrganizationModel) => Promise<IOrganizationModel>;
  updateOrganization: (model: IOrganizationModel) => Promise<IOrganizationModel>;
  deleteOrganization: (model: IOrganizationModel) => Promise<IOrganizationModel>;
}

export const useOrganizations = (): [
  IAdminState & { initialized: boolean },
  IOrganizationController,
] => {
  const api = useApiAdminOrganizations();
  const dispatch = useAjaxWrapper();
  const [state, store] = useAdminStore();
  const [, lookup] = useLookup();
  const [initialized, setInitialized] = React.useState(false);

  const controller = React.useMemo(
    () => ({
      findAllOrganizations: async () => {
        const response = await dispatch<IOrganizationModel[]>('find-all-organizations', () =>
          api.findAllOrganizations(),
        );
        store.storeOrganizations(response.data);
        setInitialized(true);
        return response.data;
      },
      getOrganization: async (id: number) => {
        const response = await dispatch<IOrganizationModel>('get-organization', () =>
          api.getOrganization(id),
        );
        store.storeOrganizations((organizations) =>
          organizations.map((ds) => {
            if (ds.id === response.data.id) return response.data;
            return ds;
          }),
        );
        return response.data;
      },
      addOrganization: async (model: IOrganizationModel) => {
        const response = await dispatch<IOrganizationModel>('add-organization', () =>
          api.addOrganization(model),
        );
        store.storeOrganizations((organizations) => [...organizations, response.data]);
        await lookup.getLookups();
        return response.data;
      },
      updateOrganization: async (model: IOrganizationModel) => {
        const response = await dispatch<IOrganizationModel>('update-organization', () =>
          api.updateOrganization(model),
        );
        store.storeOrganizations((organizations) =>
          organizations.map((ds) => {
            if (ds.id === response.data.id) return response.data;
            return ds;
          }),
        );
        await lookup.getLookups();
        return response.data;
      },
      deleteOrganization: async (model: IOrganizationModel) => {
        const response = await dispatch<IOrganizationModel>('delete-organization', () =>
          api.deleteOrganization(model),
        );
        store.storeOrganizations((organizations) =>
          organizations.filter((ds) => ds.id !== response.data.id),
        );
        await lookup.getLookups();
        return response.data;
      },
    }),
    [api, dispatch, lookup, store],
  );

  return [{ ...state, initialized }, controller];
};
