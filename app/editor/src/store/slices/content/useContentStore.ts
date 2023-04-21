import { advancedSearchKeys } from 'features/content/list-view/constants';
import {
  IContentListAdvancedFilter,
  IContentListFilter,
} from 'features/content/list-view/interfaces';
import { IMorningReportsFilter } from 'features/content/morning-reports/interfaces';
import React from 'react';
import { ActionDelegate, useAppDispatch, useAppSelector } from 'store';
import { IContentModel, IPaged } from 'tno-core';

import {
  addContent,
  removeContent,
  storeContent,
  storeFilter,
  storeFilterAdvanced,
  storeFilterMorningReport,
  storeFilterMorningReportAdvanced,
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
  storeFilterMorningReport: (
    filter: IMorningReportsFilter | ActionDelegate<IMorningReportsFilter>,
  ) => void;
  storeFilterMorningReportAdvanced: (
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
      storeFilterMorningReport: (
        filter: IMorningReportsFilter | ActionDelegate<IMorningReportsFilter>,
      ) => {
        if (typeof filter === 'function')
          dispatch(storeFilterMorningReport(filter(state.filterMorningReports)));
        else dispatch(storeFilterMorningReport(filter));
      },
      storeFilterMorningReportAdvanced: (
        filter: IContentListAdvancedFilter | ActionDelegate<IContentListAdvancedFilter>,
      ) => {
        if (typeof filter === 'function')
          dispatch(storeFilterMorningReportAdvanced(filter(filterAdvanced)));
        else dispatch(storeFilterMorningReportAdvanced(filter));
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
    [dispatch, state.filter, state.filterMorningReports],
  );

  return [state, controller];
};
