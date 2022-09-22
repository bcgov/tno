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
        const result = await dispatch<ILicenseModel[]>('find-all-licenses', () =>
          api.findAllLicenses(),
        );
        store.storeLicenses(result);
        return result;
      },
      getLicense: async (id: number) => {
        const result = await dispatch<ILicenseModel>('get-license', () => api.getLicense(id));
        store.storeLicenses(
          state.licenses.map((ds) => {
            if (ds.id === result.id) return result;
            return ds;
          }),
        );
        return result;
      },
      addLicense: async (model: ILicenseModel) => {
        const result = await dispatch<ILicenseModel>('add-license', () => api.addLicense(model));
        store.storeLicenses([...state.licenses, result]);
        return result;
      },
      updateLicense: async (model: ILicenseModel) => {
        const result = await dispatch<ILicenseModel>('update-license', () =>
          api.updateLicense(model),
        );
        store.storeLicenses(
          state.licenses.map((ds) => {
            if (ds.id === result.id) return result;
            return ds;
          }),
        );
        return result;
      },
      deleteLicense: async (model: ILicenseModel) => {
        const result = await dispatch<ILicenseModel>('delete-license', () =>
          api.deleteLicense(model),
        );
        store.storeLicenses(state.licenses.filter((ds) => ds.id !== result.id));
        return result;
      },
    }),
    // The state.licenses will cause it to fire twice!
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [dispatch, store, api],
  );

  return [state, controller];
};
