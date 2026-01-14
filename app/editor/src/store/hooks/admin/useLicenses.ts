import React from 'react';
import { useAjaxWrapper, useLookup } from 'store/hooks';
import { type IAdminState, useAdminStore } from 'store/slices';
import { type ILicenseModel, useApiAdminLicenses } from 'tno-core';

interface ILicenseController {
  findAllLicenses: () => Promise<ILicenseModel[]>;
  getLicense: (id: number) => Promise<ILicenseModel>;
  addLicense: (model: ILicenseModel) => Promise<ILicenseModel>;
  updateLicense: (model: ILicenseModel) => Promise<ILicenseModel>;
  deleteLicense: (model: ILicenseModel) => Promise<ILicenseModel>;
}

export const useLicenses = (): [IAdminState, ILicenseController] => {
  const api = useApiAdminLicenses();
  const dispatch = useAjaxWrapper();
  const [state, store] = useAdminStore();
  const [, lookup] = useLookup();

  const controller = React.useMemo(
    () => ({
      findAllLicenses: async () => {
        const response = await dispatch<ILicenseModel[]>(
          'find-all-licenses',
          async () => await api.findAllLicenses(),
        );
        store.storeLicenses(response.data);
        return response.data;
      },
      getLicense: async (id: number) => {
        const response = await dispatch<ILicenseModel>(
          'get-license',
          async () => await api.getLicense(id),
        );
        store.storeLicenses((licenses) =>
          licenses.map((ds) => {
            if (ds.id === response.data.id) return response.data;
            return ds;
          }),
        );
        return response.data;
      },
      addLicense: async (model: ILicenseModel) => {
        const response = await dispatch<ILicenseModel>(
          'add-license',
          async () => await api.addLicense(model),
        );
        store.storeLicenses((licenses) => [...licenses, response.data]);
        await lookup.getLookups();
        return response.data;
      },
      updateLicense: async (model: ILicenseModel) => {
        const response = await dispatch<ILicenseModel>(
          'update-license',
          async () => await api.updateLicense(model),
        );
        store.storeLicenses((licenses) =>
          licenses.map((ds) => {
            if (ds.id === response.data.id) return response.data;
            return ds;
          }),
        );
        await lookup.getLookups();
        return response.data;
      },
      deleteLicense: async (model: ILicenseModel) => {
        const response = await dispatch<ILicenseModel>(
          'delete-license',
          async () => await api.deleteLicense(model),
        );
        store.storeLicenses((licenses) => licenses.filter((ds) => ds.id !== response.data.id));
        await lookup.getLookups();
        return response.data;
      },
    }),
    [dispatch, store, api, lookup],
  );

  return [state, controller];
};
