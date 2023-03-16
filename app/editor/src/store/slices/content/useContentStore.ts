import {
  IContentListAdvancedFilter,
  IContentListFilter,
} from 'features/content/list-view/interfaces';
import { IMorningReportFilter } from 'features/content/morning-report/interfaces';
import { IContentModel, IPaged } from 'hooks/api-editor';
import React from 'react';
import { ActionDelegate, useAppDispatch, useAppSelector } from 'store';

import {
  addContent,
  removeContent,
  storeContent,
  storeFilter,
  storeFilterAdvanced,
  storeMorningReportFilter,
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
  storeMorningReportFilter: (
    filter: IMorningReportFilter | ActionDelegate<IMorningReportFilter>,
  ) => void;
  storeContent: (content: IPaged<IContentModel>) => void;
  addContent: (content: IContentModel[]) => void;
  updateContent: (content: IContentModel[]) => void;
  removeContent: (content: IContentModel[]) => void;
}

export const useContentStore = (props?: IContentProps): [IContentState, IContentStore] => {
  const dispatch = useAppDispatch();
  const state = useAppSelector((store) => store.content);

  const controller = React.useMemo(
    () => ({
      storeFilter: (filter: IContentListFilter | ActionDelegate<IContentListFilter>) => {
        if (typeof filter === 'function') dispatch(storeFilter(filter(state.filter)));
        else dispatch(storeFilter(filter));
      },
      storeFilterAdvanced: (
        filter: IContentListAdvancedFilter | ActionDelegate<IContentListAdvancedFilter>,
      ) => {
        if (typeof filter === 'function')
          dispatch(storeFilterAdvanced(filter(state.filterAdvanced)));
        else dispatch(storeFilterAdvanced(filter));
      },
      storeMorningReportFilter: (
        filter: IMorningReportFilter | ActionDelegate<IMorningReportFilter>,
      ) => {
        if (typeof filter === 'function')
          dispatch(storeMorningReportFilter(filter(state.filterMorningReport)));
        else dispatch(storeMorningReportFilter(filter));
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
    [dispatch, state.filter, state.filterAdvanced, state.filterMorningReport],
  );

  return [state, controller];
};
