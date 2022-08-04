import { IUserListFilter } from 'features/admin/users/interfaces/IUserListFilter';
import {
  IActionModel,
  ICategoryModel,
  IDataSourceModel,
  IMediaTypeModel,
  IPaged,
  ISeriesModel,
  ITagModel,
  IUserModel,
} from 'hooks/api-editor';
import React from 'react';
import { useAppDispatch, useAppSelector } from 'store';

import {
  storeAdminActions,
  storeAdminCategories,
  storeAdminDataSources,
  storeAdminMediaTypes,
  storeAdminSeries,
  storeAdminTags,
  storeAdminUserFilter,
  storeAdminUsers,
} from '.';
import { IAdminState } from './interfaces';

export interface IAdminProps {}

export interface IAdminStore {
  storeUserFilter: (filter: IUserListFilter) => void;
  storeDataSources: (dataSources: IDataSourceModel[]) => void;
  storeMediaTypes: (mediaTypes: IMediaTypeModel[]) => void;
  storeCategories: (categories: ICategoryModel[]) => void;
  storeUsers: (users: IPaged<IUserModel>) => void;
  storeTags: (tags: ITagModel[]) => void;
  storeActions: (actions: IActionModel[]) => void;
  storeSeries: (series: ISeriesModel[]) => void;
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
      storeCategories: (categories: ICategoryModel[]) => {
        dispatch(storeAdminCategories(categories));
      },
      storeTags: (tags: ITagModel[]) => {
        dispatch(storeAdminTags(tags));
      },
      storeActions: (actions: IActionModel[]) => {
        dispatch(storeAdminActions(actions));
      },
      storeSeries: (series: ISeriesModel[]) => {
        dispatch(storeAdminSeries(series));
      },
      storeUserFilter: (filter: IUserListFilter) => {
        dispatch(storeAdminUserFilter(filter));
      },
    }),
    [dispatch],
  );

  return [state, controller];
};
