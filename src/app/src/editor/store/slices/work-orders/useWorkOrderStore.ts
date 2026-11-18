import React from 'react';
import { useStore } from 'react-redux';

import {
  useAppDispatch,
  useAppSelector,
  type ActionDelegate,
  type RootState,
} from '@/editor/store';
import { type IWorkOrderFilter } from '@/shared';

import { type IWorkOrderState } from './interfaces';
import { storeTranscriptFilter } from './workOrderSlice';

export interface IWorkOrderStore {
  storeTranscriptFilter: (filter: IWorkOrderFilter | ActionDelegate<IWorkOrderFilter>) => void;
}

export const useWorkOrderStore = (): [IWorkOrderState, IWorkOrderStore] => {
  const dispatch = useAppDispatch();
  const state = useAppSelector((store) => store.workOrder);
  // Updates given as a function apply to the store as it is when they are made. The 'state' this
  // render selected can be out of date by then: two requests started together would each apply their
  // update to the same old value, and the last to finish would discard the other's.
  const store = useStore<RootState>();

  const controller = React.useMemo(
    () => ({
      storeTranscriptFilter: (filter: IWorkOrderFilter | ActionDelegate<IWorkOrderFilter>) => {
        if (typeof filter === 'function') {
          dispatch(storeTranscriptFilter(filter(store.getState().workOrder.transcriptFilter)));
        } else dispatch(storeTranscriptFilter(filter));
      },
    }),
    [dispatch, store],
  );

  return [state, controller];
};
