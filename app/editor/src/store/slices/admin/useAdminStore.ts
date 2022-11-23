import { IUserListFilter } from 'features/admin/users/interfaces/IUserListFilter';
import { IWorkOrderListFilter } from 'features/work-orders/interfaces/IWorkOrderListFilter';
import {
  IActionModel,
  ICategoryModel,
  IConnectionModel,
  IDataLocationModel,
  IIngestModel,
  IIngestTypeModel,
  ILicenseModel,
  IPaged,
  IProductModel,
  ISeriesModel,
  ISourceModel,
  ITagModel,
  IUserModel,
} from 'hooks/api-editor';
import React from 'react';
import { useAppDispatch, useAppSelector } from 'store';

import {
  storeAdminActions,
  storeAdminCategories,
  storeAdminConnections,
  storeAdminDataLocations,
  storeAdminIngests,
  storeAdminIngestTypes,
  storeAdminLicenses,
  storeAdminProducts,
  storeAdminSeries,
  storeAdminSources,
  storeAdminTags,
  storeAdminUserFilter,
  storeAdminUsers,
} from '.';
import { IAdminState } from './interfaces';

export interface IAdminProps {}

export interface IAdminStore {
  storeSources: (sources: ISourceModel[]) => void;
  storeConnections: (connections: IConnectionModel[]) => void;
  storeDataLocations: (dataLocations: IDataLocationModel[]) => void;
  storeProducts: (products: IProductModel[]) => void;
  storeLicenses: (licenses: ILicenseModel[]) => void;
  storeIngests: (ingests: IIngestModel[]) => void;
  storeIngestTypes: (ingestTypes: IIngestTypeModel[]) => void;
  storeCategories: (categories: ICategoryModel[]) => void;
  storeUserFilter: (filter: IUserListFilter) => void;
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
      storeSources: (sources: ISourceModel[]) => {
        dispatch(storeAdminSources(sources));
      },
      storeConnections: (connections: IConnectionModel[]) => {
        dispatch(storeAdminConnections(connections));
      },
      storeDataLocations: (dataLocations: IDataLocationModel[]) => {
        dispatch(storeAdminDataLocations(dataLocations));
      },
      storeProducts: (products: IProductModel[]) => {
        dispatch(storeAdminProducts(products));
      },
      storeLicenses: (licenses: ILicenseModel[]) => {
        dispatch(storeAdminLicenses(licenses));
      },
      storeIngests: (ingests: IIngestModel[]) => {
        dispatch(storeAdminIngests(ingests));
      },
      storeIngestTypes: (ingestTypes: IIngestTypeModel[]) => {
        dispatch(storeAdminIngestTypes(ingestTypes));
      },
      storeUserFilter: (filter: IUserListFilter) => {
        dispatch(storeAdminUserFilter(filter));
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
    }),
    [dispatch],
  );

  return [state, controller];
};
