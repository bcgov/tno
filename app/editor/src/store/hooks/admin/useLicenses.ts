import { ILicenseModel, useApiAdminLicenses } from 'hooks/api-editor';
import React from 'react';
import { useApiDispatcher } from 'store/hooks';
import { IAdminState, useAdminStore } from 'store/slices';

interface ILicenseController {
  findAllLicenses: () => Promise<ILicenseModel[]>;
  getLicense: (id: number) => Promise<ILicenseModel>;
  addLicense: (model: ILicenseModel) => Promise<ILicenseModel>;
  updateLicense: (model: ILicenseModel) => Promise<ILicenseModel>;
  deleteLicense: (model: ILicenseModel) => Promise<ILicenseModel>;
}

export const useLicenses = (): [IAdminState, ILicenseController] => {
  const api = useApiAdminLicenses();
  const dispatch = useApiDispatcher();
  const [state, store] = useAdminStore();

  const controller = React.useMemo(
    () => ({
      findAllLicenses: async () => {
        const response = await dispatch<ILicenseModel[]>('find-all-licenses', () =>
          api.findAllLicenses(),
        );
        store.storeLicenses(response.data);
        return response.data;
      },
      getLicense: async (id: number) => {
        const response = await dispatch<ILicenseModel>('get-license', () => api.getLicense(id));
        store.storeLicenses(
          state.licenses.map((ds) => {
            if (ds.id === response.data.id) return response.data;
            return ds;
          }),
        );
        return response.data;
      },
      addLicense: async (model: ILicenseModel) => {
        const response = await dispatch<ILicenseModel>('add-license', () => api.addLicense(model));
        store.storeLicenses([...state.licenses, response.data]);
        return response.data;
      },
      updateLicense: async (model: ILicenseModel) => {
        const response = await dispatch<ILicenseModel>('update-license', () =>
          api.updateLicense(model),
        );
        store.storeLicenses(
          state.licenses.map((ds) => {
            if (ds.id === response.data.id) return response.data;
            return ds;
          }),
        );
        return response.data;
      },
      deleteLicense: async (model: ILicenseModel) => {
        const response = await dispatch<ILicenseModel>('delete-license', () =>
          api.deleteLicense(model),
        );
        store.storeLicenses(state.licenses.filter((ds) => ds.id !== response.data.id));
        return response.data;
      },
    }),
    // The state.licenses will cause it to fire twice!
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [dispatch, store, api],
  );

  return [state, controller];
};
