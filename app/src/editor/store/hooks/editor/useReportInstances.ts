import React from 'react';
import { useAjaxWrapper } from 'store/hooks';
import { type IReportResultModel, useApiEditorReportInstances } from 'tno-core';

interface IReportInstanceController {
  viewReportInstance: (
    reportInstanceId: number,
    regenerate?: boolean,
  ) => Promise<IReportResultModel>;
}

export const useReportInstances = (): [IReportInstanceController] => {
  const api = useApiEditorReportInstances();
  const dispatch = useAjaxWrapper();

  const controller = React.useMemo(
    () => ({
      viewReportInstance: async (reportInstanceId: number, regenerate: boolean = false) => {
        const response = await dispatch<IReportResultModel>(
          'view-report-instance',
          async () => await api.viewReportInstance(reportInstanceId, regenerate),
        );
        return response.data;
      },
    }),
    [api, dispatch],
  );

  return [controller];
};
