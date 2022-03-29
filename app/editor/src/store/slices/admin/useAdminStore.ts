import { IDataSourceModel } from 'hooks/api-editor';
import React from 'react';
import { useAppDispatch, useAppSelector } from 'store';

import { storeDataSources } from '.';
import { IAdminState } from './interfaces';

export interface IAdminProps {}

export interface IAdminStore {
  storeDataSources: (dataSources: IDataSourceModel[]) => void;
}

export const useAdminStore = (props?: IAdminProps): [IAdminState, IAdminStore] => {
  const dispatch = useAppDispatch();
  const state = useAppSelector((store) => store.admin);

  const controller = React.useMemo(
    () => ({
      storeDataSources: (dataSources: IDataSourceModel[]) => {
        dispatch(storeDataSources(dataSources));
      },
    }),
    [dispatch],
  );

  return [state, controller];
};
