import { IDataSourceModel, IMediaTypeModel } from 'hooks/api-editor';
import React from 'react';
import { useAppDispatch, useAppSelector } from 'store';

import { storeAdminDataSources, storeAdminMediaTypes } from '.';
import { IAdminState } from './interfaces';

export interface IAdminProps {}

export interface IAdminStore {
  storeDataSources: (dataSources: IDataSourceModel[]) => void;
  storeMediaTypes: (mediaTypes: IMediaTypeModel[]) => void;
}

export const useAdminStore = (props?: IAdminProps): [IAdminState, IAdminStore] => {
  const dispatch = useAppDispatch();
  const state = useAppSelector((store) => store.admin);

  const controller = React.useMemo(
    () => ({
      storeDataSources: (dataSources: IDataSourceModel[]) => {
        dispatch(storeAdminDataSources(dataSources));
      },
      storeMediaTypes: (mediaTypes: IMediaTypeModel[]) => {
        dispatch(storeAdminMediaTypes(mediaTypes));
      },
    }),
    [dispatch],
  );

  return [state, controller];
};
