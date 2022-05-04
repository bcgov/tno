import {
  IContentListAdvancedFilter,
  IContentListFilter,
} from 'features/content/list-view/interfaces';
import { IContentModel, IPaged } from 'hooks/api-editor';
import React from 'react';
import { useAppDispatch, useAppSelector } from 'store';

import { storeContent, storeFilter, storeFilterAdvanced } from '.';
import { IContentState } from './interfaces';

export interface IContentProps {
  filter: IContentListFilter;
}

export interface IContentStore {
  storeFilter: (filter: IContentListFilter) => void;
  storeFilterAdvanced: (filter: IContentListAdvancedFilter) => void;
  storeContent: (content: IPaged<IContentModel>) => void;
}

export const useContentStore = (props?: IContentProps): [IContentState, IContentStore] => {
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
      storeContent: (content: IPaged<IContentModel>) => {
        dispatch(storeContent(content));
      },
    }),
    [dispatch],
  );

  return [state, controller];
};
