import React from 'react';
import { useStore } from 'react-redux';

import { IReportResultForm } from '@/subscriber/features/my-reports/interfaces';
import { ActionDelegate, RootState, useAppDispatch, useAppSelector } from '@/subscriber/store';

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
  // Updates given as a function apply to the store as it is when they are made. The 'state' this
  // render selected can be out of date by then: two requests started together would each apply their
  // update to the same old value, and the last to finish would discard the other's.
  const store = useStore<RootState>();

  const controller = React.useMemo(
    () => ({
      storeReportView: (
        output: IReportResultForm | undefined | ActionDelegate<IReportResultForm | undefined>,
      ) => {
        if (typeof output === 'function') {
          dispatch(storeReportView(output(store.getState().reports.reportView)));
        } else dispatch(storeReportView(output));
      },
    }),
    [dispatch, store],
  );

  return [state, controller];
};
