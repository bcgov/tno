import { IReportResultForm } from 'features/my-reports/interfaces';
import React from 'react';
import { ActionDelegate, useAppDispatch, useAppSelector } from 'store';

import { storeReportView } from '.';
import { IReportsState } from './interfaces';

export interface IReportsStore {
  storeReportView: (
    output: IReportResultForm | undefined | ActionDelegate<IReportResultForm | undefined>,
  ) => void;
}

export const useReportsStore = (): [IReportsState, IReportsStore] => {
  const dispatch = useAppDispatch();
  const state = useAppSelector((store) => store.reports);

  const controller = React.useMemo(
    () => ({
      storeReportView: (
        output: IReportResultForm | undefined | ActionDelegate<IReportResultForm | undefined>,
      ) => {
        if (typeof output === 'function') {
          dispatch(storeReportView(output(state.reportView)));
        } else dispatch(storeReportView(output));
      },
    }),
    [dispatch, state.reportView],
  );

  return [state, controller];
};
