import React from 'react';
import { useAjaxWrapper, useLookup } from 'store/hooks';
import { IAdminState, useAdminStore } from 'store/slices';
import {
  IChartRequestModel,
  IChartResultModel,
  IChartTemplateModel,
  useApiAdminChartTemplates,
} from 'tno-core';

interface IChartTemplateController {
  findAllChartTemplates: () => Promise<IChartTemplateModel[]>;
  getChartTemplate: (id: number) => Promise<IChartTemplateModel>;
  addChartTemplate: (model: IChartTemplateModel) => Promise<IChartTemplateModel>;
  updateChartTemplate: (model: IChartTemplateModel) => Promise<IChartTemplateModel>;
  deleteChartTemplate: (model: IChartTemplateModel) => Promise<IChartTemplateModel>;
  previewJson: (model: IChartRequestModel) => Promise<IChartResultModel>;
  previewBase64: (model: IChartRequestModel) => Promise<string>;
  previewImage: (model: IChartRequestModel) => Promise<any>;
}

export const useChartTemplates = (): [
  IAdminState & { initialized: boolean },
  IChartTemplateController,
] => {
  const api = useApiAdminChartTemplates();
  const dispatch = useAjaxWrapper();
  const [state, store] = useAdminStore();
  const [, lookup] = useLookup();
  const [initialized, setInitialized] = React.useState(false);

  const controller = React.useMemo(
    () => ({
      findAllChartTemplates: async () => {
        const response = await dispatch<IChartTemplateModel[]>('find-all-chart-templates', () =>
          api.findAllChartTemplates(),
        );
        store.storeChartTemplates(response.data);
        setInitialized(true);
        return response.data;
      },
      getChartTemplate: async (id: number) => {
        const response = await dispatch<IChartTemplateModel>('get-chart-template', () =>
          api.getChartTemplate(id),
        );
        store.storeChartTemplates((chartTemplates) =>
          chartTemplates.map((ds) => {
            if (ds.id === response.data.id) return response.data;
            return ds;
          }),
        );
        return response.data;
      },
      addChartTemplate: async (model: IChartTemplateModel) => {
        const response = await dispatch<IChartTemplateModel>('add-chart-template', () =>
          api.addChartTemplate(model),
        );
        store.storeChartTemplates((chartTemplates) => [...chartTemplates, response.data]);
        await lookup.getLookups();
        return response.data;
      },
      updateChartTemplate: async (model: IChartTemplateModel) => {
        const response = await dispatch<IChartTemplateModel>('update-chart-template', () =>
          api.updateChartTemplate(model),
        );
        store.storeChartTemplates((chartTemplates) =>
          chartTemplates.map((ds) => {
            if (ds.id === response.data.id) return response.data;
            return ds;
          }),
        );
        await lookup.getLookups();
        return response.data;
      },
      deleteChartTemplate: async (model: IChartTemplateModel) => {
        const response = await dispatch<IChartTemplateModel>('delete-chart-template', () =>
          api.deleteChartTemplate(model),
        );
        store.storeChartTemplates((chartTemplates) =>
          chartTemplates.filter((ds) => ds.id !== response.data.id),
        );
        await lookup.getLookups();
        return response.data;
      },
      previewJson: async (model: IChartRequestModel) => {
        const response = await dispatch<IChartResultModel>('preview-chart-template-json', () =>
          api.previewJson(model),
        );
        return response.data;
      },
      previewBase64: async (model: IChartRequestModel) => {
        const response = await dispatch<string>('preview-chart-template-base64', () =>
          api.previewBase64(model),
        );
        return response.data;
      },
      previewImage: async (model: IChartRequestModel) => {
        const response = await dispatch<any>('preview-chart-template-image', () =>
          api.previewImage(model),
        );
        return response.data;
      },
    }),
    [api, dispatch, lookup, store],
  );

  return [{ ...state, initialized }, controller];
};
