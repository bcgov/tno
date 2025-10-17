import { AdvancedSearchKeys } from 'features/content/constants';
import { IContentListAdvancedFilter, IContentListFilter } from 'features/content/interfaces';
import React from 'react';
import { ActionDelegate, useAppDispatch, useAppSelector } from 'store';
import { LogicalOperator } from 'tno-core';

import {
  storeContentFilter,
  storeContentFilterAdvanced,
  storeFilterPaper,
  storeFilterPaperAdvanced,
} from '.';
import { IContentState } from './interfaces';

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

var filterAdvanced: IContentListAdvancedFilter = {
  fieldType: AdvancedSearchKeys.Source,
  logicalOperator: '',
  searchTerm: '',
  secondaryFieldType: AdvancedSearchKeys.Source,
  secondaryLogicalOperator: LogicalOperator.Contains,
  secondarySearchTerm: '',
};

export const useContentStore = (props?: IContentProps): [IContentState, IContentStore] => {
  const dispatch = useAppDispatch();
  const state = useAppSelector((store) => store.content);

  // We need to do this because react useEffects are garbage.
  // Reset the value every time so that the memo has the correct value.
  filterAdvanced = state.filterAdvanced;

  const controller = React.useMemo(
    () => ({
      storeFilter: (filter: IContentListFilter | ActionDelegate<IContentListFilter>) => {
        if (typeof filter === 'function') dispatch(storeContentFilter(filter(state.filter)));
        else dispatch(storeContentFilter(filter));
      },
      storeFilterAdvanced: (
        filter: IContentListAdvancedFilter | ActionDelegate<IContentListAdvancedFilter>,
      ) => {
        if (typeof filter === 'function')
          dispatch(storeContentFilterAdvanced(filter(filterAdvanced)));
        else dispatch(storeContentFilterAdvanced(filter));
      },
      storeFilterPaper: (filter: IContentListFilter | ActionDelegate<IContentListFilter>) => {
        if (typeof filter === 'function') dispatch(storeFilterPaper(filter(state.filterPaper)));
        else dispatch(storeFilterPaper(filter));
      },
      storeFilterPaperAdvanced: (
        filter: IContentListAdvancedFilter | ActionDelegate<IContentListAdvancedFilter>,
      ) => {
        if (typeof filter === 'function')
          dispatch(storeFilterPaperAdvanced(filter(filterAdvanced)));
        else dispatch(storeFilterPaperAdvanced(filter));
      },
    }),
    [dispatch, state.filter, state.filterPaper],
  );

  return [state, controller];
};
