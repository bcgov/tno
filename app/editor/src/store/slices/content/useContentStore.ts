import {
  IContentListAdvancedFilter,
  IContentListFilter,
  ISortBy,
} from 'features/content/list-view/interfaces';
import React from 'react';
import { useAppDispatch, useAppSelector } from 'store';

import { storeFilter, storeFilterAdvanced, storeSortBy } from '.';
import { IContentState } from './interfaces';

export interface IContentStore {
  storeFilter: (filter: IContentListFilter) => void;
  storeFilterAdvanced: (filter: IContentListAdvancedFilter) => void;
  storeSortBy: (sortBy: ISortBy[]) => void;
}

export const useContentStore = (): [IContentState, IContentStore] => {
  const dispatch = useAppDispatch();
  const state = useAppSelector((store) => store.content);

  const controller = React.useMemo(
    () => ({
      storeFilter: (filter: IContentListFilter) => {
        dispatch(storeFilter(filter));
      },
      storeFilterAdvanced: (filter: IContentListAdvancedFilter) => {
        dispatch(storeFilterAdvanced(filter));
      },
      storeSortBy: (sortBy: ISortBy[]) => {
        dispatch(storeSortBy(sortBy));
      },
    }),
    [dispatch],
  );

  return [state, controller];
};
