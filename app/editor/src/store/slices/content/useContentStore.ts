import {
  IContentListAdvancedFilter,
  IContentListFilter,
  ISortBy,
} from 'features/content/list-view/interfaces';
import React from 'react';
import { useAppDispatch, useAppSelector } from 'store';
import { useDeepCompareCallback, useDeepCompareMemo } from 'tno-core';

import { storeFilter, storeFilterAdvanced, storeSortBy } from '.';
import { IContentState } from './interfaces';

export interface IContentStore {
  storeFilter: (
    filter: IContentListFilter | ((filter: IContentListFilter) => IContentListFilter),
  ) => void;
  storeFilterAdvanced: (filter: IContentListAdvancedFilter) => void;
  storeSortBy: (sortBy: ISortBy[]) => void;
}

export const useContentStore = (): [IContentState, IContentStore] => {
  const dispatch = useAppDispatch();
  const state = useAppSelector((store) => store.content);

  const _storeFilter = useDeepCompareCallback(
    (filter: IContentListFilter | ((filter: IContentListFilter) => IContentListFilter)) => {
      if (typeof filter === 'function') dispatch(storeFilter(filter(state.filter)));
      else dispatch(storeFilter(filter));
    },
    [dispatch, state.filter],
  );

  const _storeFilterAdvanced = React.useCallback(
    (filter: IContentListAdvancedFilter) => {
      dispatch(storeFilterAdvanced(filter));
    },
    [dispatch],
  );

  const _storeSortBy = React.useCallback(
    (sortBy: ISortBy[]) => {
      dispatch(storeSortBy(sortBy));
    },
    [dispatch],
  );

  return [
    state,
    useDeepCompareMemo(
      () => ({
        storeFilter: _storeFilter,
        storeFilterAdvanced: _storeFilterAdvanced,
        storeSortBy: _storeSortBy,
      }),
      [_storeFilter, _storeFilterAdvanced, _storeSortBy],
    ),
  ];
};
