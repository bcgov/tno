import {
  IActionModel,
  ICacheModel,
  ICategoryModel,
  IClaimModel,
  IContentTypeModel,
  IDataLocationModel,
  IDataSourceModel,
  ILicenseModel,
  IMediaTypeModel,
  IRoleModel,
  ISeriesModel,
  ISourceActionModel,
  ISourceMetricModel,
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
  storeClaims,
  storeContentTypes,
  storeDataLocations,
  storeDataSources,
  storeLicenses,
  storeMediaTypes,
  storeRoles,
  storeSeries,
  storeSourceActions,
  storeSourceMetrics,
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
  storeClaims: (claims: IClaimModel[]) => void;
  storeContentTypes: (contentTypes: IContentTypeModel[]) => void;
  storeDataLocations: (dataLocations: IDataLocationModel[]) => void;
  storeDataSources: (dataSources: IDataSourceModel[]) => void;
  storeLicenses: (licenses: ILicenseModel[]) => void;
  storeMediaTypes: (mediaTypes: IMediaTypeModel[]) => void;
  storeRoles: (roles: IRoleModel[]) => void;
  storeSeries: (series: ISeriesModel[]) => void;
  storeSourceActions: (actions: ISourceActionModel[]) => void;
  storeSourceMetrics: (metrics: ISourceMetricModel[]) => void;
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
      storeClaims: (claims: IClaimModel[]) => {
        dispatch(storeClaims(claims));
      },
      storeContentTypes: (contentTypes: IContentTypeModel[]) => {
        dispatch(storeContentTypes(contentTypes));
      },
      storeDataLocations: (dataLocations: IDataLocationModel[]) => {
        dispatch(storeDataLocations(dataLocations));
      },
      storeDataSources: (dataSources: IDataSourceModel[]) => {
        dispatch(storeDataSources(dataSources));
      },
      storeLicenses: (licenses: ILicenseModel[]) => {
        dispatch(storeLicenses(licenses));
      },
      storeMediaTypes: (mediaTypes: IMediaTypeModel[]) => {
        dispatch(storeMediaTypes(mediaTypes));
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
      storeSourceMetrics: (metrics: ISourceMetricModel[]) => {
        dispatch(storeSourceMetrics(metrics));
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
