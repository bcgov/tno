import React from 'react';
import { useAjaxWrapper } from 'store/hooks';
import { IReportTemplateModel, useApiSubscriberReportTemplates } from 'tno-core';

interface IReportController {
  getReportTemplates: () => Promise<IReportTemplateModel[]>;
  getReportTemplate: (id: number) => Promise<IReportTemplateModel | undefined>;
}

export const useReportTemplates = (): [IReportController] => {
  const api = useApiSubscriberReportTemplates();
  const dispatch = useAjaxWrapper();

  const controller = React.useMemo(
    () => ({
      getReportTemplates: async () => {
        const response = await dispatch<IReportTemplateModel[]>('get-report-templates', () =>
          api.getReportTemplates(),
        );
        return response.data;
      },
      getReportTemplate: async (id: number) => {
        const response = await dispatch<IReportTemplateModel | undefined>(
          'get-report-template',
          () => api.getReportTemplate(id),
        );
        return response.data;
      },
    }),
    [api, dispatch],
  );

  return [controller];
};
