import React from 'react';
import { useApiEditorReportInstances, type IReportResultModel } from 'tno-core';

import { useAjaxWrapper } from '@/editor/store/hooks';

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
