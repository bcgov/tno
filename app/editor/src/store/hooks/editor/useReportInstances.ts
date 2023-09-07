import React from 'react';
import { useAjaxWrapper } from 'store/hooks';
import { IReportResultModel, useApiEditorReportInstances } from 'tno-core';

interface IReportInstanceController {
  previewReportInstance: (reportInstanceId: number) => Promise<IReportResultModel>;
}

export const useReportInstances = (): [IReportInstanceController] => {
  const api = useApiEditorReportInstances();
  const dispatch = useAjaxWrapper();

  const controller = React.useMemo(
    () => ({
      previewReportInstance: async (reportInstanceId: number) => {
        const response = await dispatch<IReportResultModel>('preview-report-instance', () =>
          api.previewReportInstance(reportInstanceId),
        );
        return response.data;
      },
    }),
    [api, dispatch],
  );

  return [controller];
};
