import {
  IActionModel,
  ICacheModel,
  ICategoryModel,
  IIngestTypeModel,
  ILicenseModel,
  IMetricModel,
  IProductModel,
  IRoleModel,
  ISeriesModel,
  ISourceActionModel,
  ISourceModel,
  ITagModel,
  ITonePoolModel,
  IUserModel,
} from 'hooks/api-editor';
import React from 'react';
import { useAppDispatch, useAppSelector } from 'store';

import {
  storeActions,
  storeCache,
  storeCategories,
  storeIngestTypes,
  storeLicenses,
  storeMetrics,
  storeProducts,
  storeRoles,
  storeSeries,
  storeSourceActions,
  storeSources,
  storeTags,
  storeTonePools,
  storeUsers,
  updateCache,
} from '.';
import { ILookupState } from './interfaces';

export interface ILookupStore {
  storeCache: (cache: ICacheModel[]) => void;
  updateCache: (cache: ICacheModel) => void;
  storeActions: (actions: IActionModel[]) => void;
  storeCategories: (categories: ICategoryModel[]) => void;
  storeProducts: (contentTypes: IProductModel[]) => void;
  storeSources: (sources: ISourceModel[]) => void;
  storeLicenses: (licenses: ILicenseModel[]) => void;
  storeIngestTypes: (ingestTypes: IIngestTypeModel[]) => void;
  storeRoles: (roles: IRoleModel[]) => void;
  storeSeries: (series: ISeriesModel[]) => void;
  storeSourceActions: (actions: ISourceActionModel[]) => void;
  storeMetrics: (metrics: IMetricModel[]) => void;
  storeTags: (tags: ITagModel[]) => void;
  storeTonePools: (contentTypes: ITonePoolModel[]) => void;
  storeUsers: (users: IUserModel[]) => void;
}

export const useLookupStore = (): [ILookupState, ILookupStore] => {
  const dispatch = useAppDispatch();
  const state = useAppSelector((store) => store.lookup);

  const controller = React.useMemo(
    () => ({
      storeCache: (cache: ICacheModel[]) => {
        dispatch(storeCache(cache));
      },
      updateCache: (cache: ICacheModel) => {
        dispatch(updateCache(cache));
      },
      storeActions: (actions: IActionModel[]) => {
        dispatch(storeActions(actions));
      },
      storeCategories: (categories: ICategoryModel[]) => {
        dispatch(storeCategories(categories));
      },
      storeProducts: (contentTypes: IProductModel[]) => {
        dispatch(storeProducts(contentTypes));
      },
      storeSources: (sources: ISourceModel[]) => {
        dispatch(storeSources(sources));
      },
      storeLicenses: (licenses: ILicenseModel[]) => {
        dispatch(storeLicenses(licenses));
      },
      storeIngestTypes: (ingestTypes: IIngestTypeModel[]) => {
        dispatch(storeIngestTypes(ingestTypes));
      },
      storeRoles: (roles: IRoleModel[]) => {
        dispatch(storeRoles(roles));
      },
      storeSeries: (series: ISeriesModel[]) => {
        dispatch(storeSeries(series));
      },
      storeSourceActions: (actions: ISourceActionModel[]) => {
        dispatch(storeSourceActions(actions));
      },
      storeMetrics: (metrics: IMetricModel[]) => {
        dispatch(storeMetrics(metrics));
      },
      storeTags: (tags: ITagModel[]) => {
        dispatch(storeTags(tags));
      },
      storeTonePools: (tonePools: ITonePoolModel[]) => {
        dispatch(storeTonePools(tonePools));
      },
      storeUsers: (users: IUserModel[]) => {
        dispatch(storeUsers(users));
      },
    }),
    [dispatch],
  );

  return [state, controller];
};
