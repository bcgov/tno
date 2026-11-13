import React from 'react';

import { useAppDispatch, useAppSelector, type ActionDelegate } from '@/editor/store';
import { type IWorkOrderFilter } from '@/shared';

import { type IWorkOrderState } from './interfaces';
import { storeTranscriptFilter } from './workOrderSlice';

export interface IWorkOrderStore {
  storeTranscriptFilter: (filter: IWorkOrderFilter | ActionDelegate<IWorkOrderFilter>) => void;
}

export const useWorkOrderStore = (): [IWorkOrderState, IWorkOrderStore] => {
  const dispatch = useAppDispatch();
  const state = useAppSelector((store) => store.workOrder);

  const controller = React.useMemo(
    () => ({
      storeTranscriptFilter: (filter: IWorkOrderFilter | ActionDelegate<IWorkOrderFilter>) => {
        if (typeof filter === 'function') {
          dispatch(storeTranscriptFilter(filter(state.transcriptFilter)));
        } else dispatch(storeTranscriptFilter(filter));
      },
    }),
    [dispatch, state.transcriptFilter],
  );

  return [state, controller];
};
