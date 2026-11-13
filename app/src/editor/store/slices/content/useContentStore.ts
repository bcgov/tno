import React from 'react';
import { useStore } from 'react-redux';

import {
  type IContentListAdvancedFilter,
  type IContentListFilter,
} from '@/editor/features/content/interfaces';
import {
  useAppDispatch,
  useAppSelector,
  type ActionDelegate,
  type RootState,
} from '@/editor/store';

import {
  storeContentFilter,
  storeContentFilterAdvanced,
  storeFilterPaper,
  storeFilterPaperAdvanced,
} from '.';
import { type IContentState } from './interfaces';

export interface IContentProps {
  filter: IContentListFilter;
}

export interface IContentStore {
  storeFilter: (filter: IContentListFilter | ActionDelegate<IContentListFilter>) => void;
  storeFilterAdvanced: (
    filter: IContentListAdvancedFilter | ActionDelegate<IContentListAdvancedFilter>,
  ) => void;
  storeFilterPaper: (filter: IContentListFilter | ActionDelegate<IContentListFilter>) => void;
  storeFilterPaperAdvanced: (
    filter: IContentListAdvancedFilter | ActionDelegate<IContentListAdvancedFilter>,
  ) => void;
}

export const useContentStore = (props?: IContentProps): [IContentState, IContentStore] => {
  const dispatch = useAppDispatch();
  const state = useAppSelector((store) => store.content);
  // Updates given as a function apply to the store as it is when they are made. The 'state' this
  // render selected can be out of date by then: two requests started together would each apply their
  // update to the same old value, and the last to finish would discard the other's.
  const store = useStore<RootState>();

  const controller = React.useMemo(
    () => ({
      storeFilter: (filter: IContentListFilter | ActionDelegate<IContentListFilter>) => {
        if (typeof filter === 'function')
          dispatch(storeContentFilter(filter(store.getState().content.filter)));
        else dispatch(storeContentFilter(filter));
      },
      storeFilterAdvanced: (
        filter: IContentListAdvancedFilter | ActionDelegate<IContentListAdvancedFilter>,
      ) => {
        if (typeof filter === 'function') {
          dispatch(storeContentFilterAdvanced(filter(store.getState().content.filterAdvanced)));
        } else dispatch(storeContentFilterAdvanced(filter));
      },
      storeFilterPaper: (filter: IContentListFilter | ActionDelegate<IContentListFilter>) => {
        if (typeof filter === 'function')
          dispatch(storeFilterPaper(filter(store.getState().content.filterPaper)));
        else dispatch(storeFilterPaper(filter));
      },
      storeFilterPaperAdvanced: (
        filter: IContentListAdvancedFilter | ActionDelegate<IContentListAdvancedFilter>,
      ) => {
        if (typeof filter === 'function') {
          dispatch(storeFilterPaperAdvanced(filter(store.getState().content.filterPaperAdvanced)));
        } else dispatch(storeFilterPaperAdvanced(filter));
      },
    }),
    [dispatch, store],
  );

  return [state, controller];
};
