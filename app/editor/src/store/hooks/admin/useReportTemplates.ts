import React from 'react';
import { useAjaxWrapper, useLookup } from 'store/hooks';
import { IAdminState, useAdminStore } from 'store/slices';
import { IReportTemplateModel, useApiAdminReportTemplates } from 'tno-core';

interface IReportTemplateController {
  findAllReportTemplates: () => Promise<IReportTemplateModel[]>;
  getReportTemplate: (id: number) => Promise<IReportTemplateModel>;
  addReportTemplate: (model: IReportTemplateModel) => Promise<IReportTemplateModel>;
  updateReportTemplate: (model: IReportTemplateModel) => Promise<IReportTemplateModel>;
  deleteReportTemplate: (model: IReportTemplateModel) => Promise<IReportTemplateModel>;
}

export const useReportTemplates = (): [IAdminState, IReportTemplateController] => {
  const api = useApiAdminReportTemplates();
  const dispatch = useAjaxWrapper();
  const [state, store] = useAdminStore();
  const [, lookup] = useLookup();

  const controller = React.useMemo(
    () => ({
      findAllReportTemplates: async () => {
        const response = await dispatch<IReportTemplateModel[]>('find-all-report-templates', () =>
          api.findAllReportTemplates(),
        );
        store.storeReportTemplates(response.data);
        return response.data;
      },
      getReportTemplate: async (id: number) => {
        const response = await dispatch<IReportTemplateModel>('get-report-template', () =>
          api.getReportTemplate(id),
        );
        store.storeReportTemplates((reportTemplates) =>
          reportTemplates.map((ds) => {
            if (ds.id === response.data.id) return response.data;
            return ds;
          }),
        );
        return response.data;
      },
      addReportTemplate: async (model: IReportTemplateModel) => {
        const response = await dispatch<IReportTemplateModel>('add-report-template', () =>
          api.addReportTemplate(model),
        );
        store.storeReportTemplates((reportTemplates) => [...reportTemplates, response.data]);
        await lookup.getLookups();
        return response.data;
      },
      updateReportTemplate: async (model: IReportTemplateModel) => {
        const response = await dispatch<IReportTemplateModel>('update-report-template', () =>
          api.updateReportTemplate(model),
        );
        store.storeReportTemplates((reportTemplates) =>
          reportTemplates.map((ds) => {
            if (ds.id === response.data.id) return response.data;
            return ds;
          }),
        );
        await lookup.getLookups();
        return response.data;
      },
      deleteReportTemplate: async (model: IReportTemplateModel) => {
        const response = await dispatch<IReportTemplateModel>('delete-report-template', () =>
          api.deleteReportTemplate(model),
        );
        store.storeReportTemplates((reportTemplates) =>
          reportTemplates.filter((ds) => ds.id !== response.data.id),
        );
        await lookup.getLookups();
        return response.data;
      },
    }),
    [api, dispatch, lookup, store],
  );

  return [state, controller];
};
