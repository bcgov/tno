import {
  IActionModel,
  ICategoryModel,
  IContentTypeModel,
  ILicenseModel,
  IMediaTypeModel,
  ISeriesModel,
  ISourceActionModel,
  ISourceMetricModel,
  ITagModel,
  ITonePoolModel,
  IUserModel,
  useApiSourceActions,
  useApiSourceMetrics,
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
import { useLookupStore } from 'store/slices';
import { ILookupState } from 'store/slices/lookup';

import { useApiDispatcher } from '..';

let isInitialized = false;

interface ILookupController {
  getActions: () => Promise<IActionModel[]>;
  getSourceActions: () => Promise<ISourceActionModel[]>;
  getSourceMetrics: () => Promise<ISourceMetricModel[]>;
  getCategories: () => Promise<ICategoryModel[]>;
  getContentTypes: () => Promise<IContentTypeModel[]>;
  getLicenses: () => Promise<ILicenseModel[]>;
  getMediaTypes: () => Promise<IMediaTypeModel[]>;
  getSeries: () => Promise<ISeriesModel[]>;
  getTags: () => Promise<ITagModel[]>;
  getTonePools: () => Promise<ITonePoolModel[]>;
  getUsers: () => Promise<IUserModel[]>;
  init: () => void;
}

export const useLookup = (): [ILookupState, ILookupController] => {
  const [state, store] = useLookupStore();
  const dispatch = useApiDispatcher();
  const actions = useApiActions();
  const sourceActions = useApiSourceActions();
  const sourceMetrics = useApiSourceMetrics();
  const categories = useApiCategories();
  const contentTypes = useApiContentTypes();
  const licenses = useApiLicenses();
  const mediaTypes = useApiMediaTypes();
  const series = useApiSeries();
  const tags = useApiTags();
  const tonePools = useApiTonePools();
  const users = useApiUsers();

  const api = React.useRef({
    getActions: async () => {
      const result = await dispatch('get-actions', () => actions.getActions());
      store.storeActions(result);
      return result;
    },
    getSourceActions: async () => {
      const result = await dispatch('get-source-actions', () => sourceActions.getActions());
      store.storeSourceActions(result);
      return result;
    },
    getSourceMetrics: async () => {
      const result = await dispatch('get-source-metrics', () => sourceMetrics.getMetrics());
      store.storeSourceMetrics(result);
      return result;
    },
    getCategories: async () => {
      const result = await dispatch('get-categories', () => categories.getCategories());
      store.storeCategories(result);
      return result;
    },
    getContentTypes: async () => {
      const result = await dispatch('get-content-types', () => contentTypes.getContentTypes());
      store.storeContentTypes(result);
      return result;
    },
    getLicenses: async () => {
      const result = await dispatch('get-license', () => licenses.getLicenses());
      store.storeLicenses(result);
      return result;
    },
    getMediaTypes: async () => {
      const result = await dispatch('get-media-types', () => mediaTypes.getMediaTypes());
      store.storeMediaTypes(result);
      return result;
    },
    getSeries: async () => {
      const result = await dispatch('get-series', () => series.getSeries());
      store.storeSeries(result);
      return result;
    },
    getTags: async () => {
      const result = await dispatch('get-tags', () => tags.getTags());
      store.storeTags(result);
      return result;
    },
    getTonePools: async () => {
      const result = await dispatch('get-tone-pools', () => tonePools.getTonePools());
      store.storeTonePools(result);
      return result;
    },
    getUsers: async () => {
      const result = await dispatch('get-users', () => users.getUsers());
      store.storeUsers(result);
      return result;
    },
  }).current;

  const controller = React.useRef({
    ...api,
    init: () => {
      // Failures will have to manually be resolved.
      if (!state.actions.length) api.getActions();
      if (!state.sourceActions.length) api.getSourceActions();
      if (!state.sourceMetrics.length) api.getSourceMetrics();
      if (!state.categories.length) api.getCategories();
      if (!state.contentTypes.length) api.getContentTypes();
      if (!state.licenses.length) api.getLicenses();
      if (!state.mediaTypes.length) api.getMediaTypes();
      if (!state.series.length) api.getSeries();
      if (!state.tags.length) api.getTags();
      if (!state.tonePools.length) api.getTonePools();
      if (!state.users.length) api.getUsers();
    },
  }).current;

  // Initialize the first time the hook is called only.
  if (!isInitialized) {
    controller.init();
    isInitialized = true;
  }

  return [state, controller];
};
