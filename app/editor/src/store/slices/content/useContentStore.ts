import {
  IContentListAdvancedFilter,
  IContentListFilter,
} from 'features/content/list-view/interfaces';
import React from 'react';
import { useAppDispatch, useAppSelector } from 'store';
import { useDeepCompareEffect } from 'tno-core';

import { init, storeFilter, storeFilterAdvanced } from '.';
import { IContentState } from './interfaces';

export interface IContentProps {
  filter: IContentListFilter;
}

export interface IContentStore {
  storeFilter: (filter: IContentListFilter) => void;
  storeFilterAdvanced: (filter: IContentListAdvancedFilter) => void;
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
    }),
    [dispatch],
  );

  useDeepCompareEffect(() => {
    if (!state.initialized && props?.filter) {
      dispatch(init(props?.filter));
    }
  }, [controller, props?.filter]);

  return [state, controller];
};
