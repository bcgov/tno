import {
  IActionModel,
  ICategoryModel,
  IContentTypeModel,
  ILicenseModel,
  IMediaTypeModel,
  ISeriesModel,
  ITagModel,
  ITonePoolModel,
  IUserModel,
} from 'hooks/api-editor';
import {
  useApiActions,
  useApiCategories,
  useApiContentTypes,
  useApiLicenses,
  useApiMediaTypes,
  useApiSeries,
  useApiTags,
  useApiTonePools,
  useApiUsers,
} from 'hooks/api-editor';
import React from 'react';
import { ILookupStore, useLookupStore } from 'store/slices';
import { ILookupState } from 'store/slices/lookup';

interface ILookupHook {
  getActions: () => Promise<IActionModel[]>;
  getCategories: () => Promise<ICategoryModel[]>;
  getContentTypes: () => Promise<IContentTypeModel[]>;
  getLicenses: () => Promise<ILicenseModel[]>;
  getMediaTypes: () => Promise<IMediaTypeModel[]>;
  getSeries: () => Promise<ISeriesModel[]>;
  getTags: () => Promise<ITagModel[]>;
  getTonePools: () => Promise<ITonePoolModel[]>;
  getUsers: () => Promise<IUserModel[]>;
}

export const useLookup = (): [ILookupState, ILookupHook, ILookupStore] => {
  const [state, store] = useLookupStore();
  const actions = useApiActions();
  const categories = useApiCategories();
  const contentTypes = useApiContentTypes();
  const licenses = useApiLicenses();
  const mediaTypes = useApiMediaTypes();
  const series = useApiSeries();
  const tags = useApiTags();
  const tonePools = useApiTonePools();
  const users = useApiUsers();

  const getActions = React.useCallback(async () => {
    const result = await actions.getActions();
    store.storeActions(result);
    return result;
  }, [actions, store]);

  const getCategories = React.useCallback(async () => {
    const result = await categories.getCategories();
    store.storeCategories(result);
    return result;
  }, [categories, store]);

  const getContentTypes = React.useCallback(async () => {
    const result = await contentTypes.getContentTypes();
    store.storeContentTypes(result);
    return result;
  }, [contentTypes, store]);

  const getLicenses = React.useCallback(async () => {
    const result = await licenses.getLicenses();
    store.storeLicenses(result);
    return result;
  }, [licenses, store]);

  const getMediaTypes = React.useCallback(async () => {
    const result = await mediaTypes.getMediaTypes();
    store.storeMediaTypes(result);
    return result;
  }, [mediaTypes, store]);

  const getSeries = React.useCallback(async () => {
    const result = await series.getSeries();
    store.storeSeries(result);
    return result;
  }, [series, store]);

  const getTags = React.useCallback(async () => {
    const result = await tags.getTags();
    store.storeTags(result);
    return result;
  }, [tags, store]);

  const getTonePools = React.useCallback(async () => {
    const result = await tonePools.getTonePools();
    store.storeTonePools(result);
    return result;
  }, [tonePools, store]);

  const getUsers = React.useCallback(async () => {
    const result = await users.getUsers();
    store.storeUsers(result);
    return result;
  }, [users, store]);

  const hook = {
    getActions,
    getCategories,
    getContentTypes,
    getLicenses,
    getMediaTypes,
    getSeries,
    getTags,
    getTonePools,
    getUsers,
  };

  React.useEffect(() => {
    // Initialize the first time the hook is called only.
    // Failures will have to manually be resolved.
    if (!state.actions.length) hook.getActions();
    if (!state.categories.length) hook.getCategories();
    if (!state.contentTypes.length) hook.getContentTypes();
    if (!state.licenses.length) hook.getLicenses();
    if (!state.mediaTypes.length) hook.getMediaTypes();
    if (!state.series.length) hook.getSeries();
    if (!state.tags.length) hook.getTags();
    if (!state.tonePools.length) hook.getTonePools();
    if (!state.users.length) hook.getUsers();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  return [state, hook, store];
};
