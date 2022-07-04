import {
  IDataSourceModel,
  IMediaTypeModel,
  IPaged,
  IUserFilter,
  IUserModel,
} from 'hooks/api-editor';
import React from 'react';
import { useAppDispatch, useAppSelector } from 'store';

import {
  storeAdminDataSources,
  storeAdminMediaTypes,
  storeAdminUserFilter,
  storeAdminUsers,
} from '.';
import { IAdminState } from './interfaces';

export interface IAdminProps {}

export interface IAdminStore {
  storeUserFilter: (filter: IUserFilter) => void;
  storeDataSources: (dataSources: IDataSourceModel[]) => void;
  storeMediaTypes: (mediaTypes: IMediaTypeModel[]) => void;
  storeUsers: (users: IPaged<IUserModel>) => void;
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
      storeUsers: (users: IPaged<IUserModel>) => {
        dispatch(storeAdminUsers(users));
      },
      storeUserFilter: (filter: IUserFilter) => {
        dispatch(storeAdminUserFilter(filter));
      },
    }),
    [dispatch],
  );

  return [state, controller];
};
