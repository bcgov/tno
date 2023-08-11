import React from 'react';
import { useAjaxWrapper, useLookup } from 'store/hooks';
import { IAdminState, useAdminStore } from 'store/slices';
import { ISettingModel, useApiAdminSettings } from 'tno-core';

interface ISettingController {
  findAllSettings: () => Promise<ISettingModel[]>;
  getSetting: (id: number) => Promise<ISettingModel>;
  addSetting: (model: ISettingModel) => Promise<ISettingModel>;
  updateSetting: (model: ISettingModel) => Promise<ISettingModel>;
  deleteSetting: (model: ISettingModel) => Promise<ISettingModel>;
}

export const useSettings = (): [IAdminState, ISettingController] => {
  const api = useApiAdminSettings();
  const dispatch = useAjaxWrapper();
  const [state, store] = useAdminStore();
  const [, lookup] = useLookup();

  const controller = React.useMemo(
    () => ({
      findAllSettings: async () => {
        const response = await dispatch<ISettingModel[]>('find-all-settings', () =>
          api.findAllSettings(),
        );
        store.storeSettings(response.data);
        return response.data;
      },
      getSetting: async (id: number) => {
        const response = await dispatch<ISettingModel>('get-setting', () => api.getSetting(id));
        store.storeSettings((settings) =>
          settings.map((ds) => {
            if (ds.id === response.data.id) return response.data;
            return ds;
          }),
        );
        return response.data;
      },
      addSetting: async (model: ISettingModel) => {
        const response = await dispatch<ISettingModel>('add-setting', () => api.addSetting(model));
        store.storeSettings((settings) => [...settings, response.data]);
        await lookup.getLookups();
        return response.data;
      },
      updateSetting: async (model: ISettingModel) => {
        const response = await dispatch<ISettingModel>('update-setting', () =>
          api.updateSetting(model),
        );
        store.storeSettings((settings) =>
          settings.map((ds) => {
            if (ds.id === response.data.id) return response.data;
            return ds;
          }),
        );
        await lookup.getLookups();
        return response.data;
      },
      deleteSetting: async (model: ISettingModel) => {
        const response = await dispatch<ISettingModel>('delete-setting', () =>
          api.deleteSetting(model),
        );
        store.storeSettings((settings) => settings.filter((ds) => ds.id !== response.data.id));
        await lookup.getLookups();
        return response.data;
      },
    }),
    [api, dispatch, lookup, store],
  );

  return [state, controller];
};
