import { advancedSearchKeys } from 'features/content/list-view/constants';
import {
  IContentListAdvancedFilter,
  IContentListFilter,
} from 'features/content/list-view/interfaces';
import { IPaperFilter } from 'features/content/papers/interfaces';
import React from 'react';
import { ActionDelegate, useAppDispatch, useAppSelector } from 'store';
import { IContentModel, IPaged } from 'tno-core';

import {
  addContent,
  removeContent,
  storeContent,
  storeFilter,
  storeFilterAdvanced,
  storeFilterPaper,
  storeFilterPaperAdvanced,
  updateContent,
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
  storeFilterPaper: (filter: IPaperFilter | ActionDelegate<IPaperFilter>) => void;
  storeFilterPaperAdvanced: (
    filter: IContentListAdvancedFilter | ActionDelegate<IContentListAdvancedFilter>,
  ) => void;
  storeContent: (content: IPaged<IContentModel>) => void;
  addContent: (content: IContentModel[]) => void;
  updateContent: (content: IContentModel[]) => void;
  removeContent: (content: IContentModel[]) => void;
}

var filterAdvanced: IContentListAdvancedFilter = {
  fieldType: advancedSearchKeys.Source,
  logicalOperator: '',
  searchTerm: '',
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
        if (typeof filter === 'function') dispatch(storeFilter(filter(state.filter)));
        else dispatch(storeFilter(filter));
      },
      storeFilterAdvanced: (
        filter: IContentListAdvancedFilter | ActionDelegate<IContentListAdvancedFilter>,
      ) => {
        if (typeof filter === 'function') dispatch(storeFilterAdvanced(filter(filterAdvanced)));
        else dispatch(storeFilterAdvanced(filter));
      },
      storeFilterPaper: (filter: IPaperFilter | ActionDelegate<IPaperFilter>) => {
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
      storeContent: (content: IPaged<IContentModel>) => {
        dispatch(storeContent(content));
      },
      addContent: (content: IContentModel[]) => {
        dispatch(addContent(content));
      },
      updateContent: (content: IContentModel[]) => {
        dispatch(updateContent(content));
      },
      removeContent: (content: IContentModel[]) => {
        dispatch(removeContent(content));
      },
    }),
    [dispatch, state.filter, state.filterPaper],
  );

  return [state, controller];
};
