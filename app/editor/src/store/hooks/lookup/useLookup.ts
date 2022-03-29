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
}

export const useLookup = (): [ILookupState, ILookupController] => {
  const [state, store] = useLookupStore();
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

  const controller = React.useRef({
    getActions: async () => {
      const result = await actions.getActions();
      store.storeActions(result);
      return result;
    },
    getSourceActions: async () => {
      const result = await sourceActions.getActions();
      store.storeSourceActions(result);
      return result;
    },
    getSourceMetrics: async () => {
      const result = await sourceMetrics.getMetrics();
      store.storeSourceMetrics(result);
      return result;
    },
    getCategories: async () => {
      const result = await categories.getCategories();
      store.storeCategories(result);
      return result;
    },
    getContentTypes: async () => {
      const result = await contentTypes.getContentTypes();
      store.storeContentTypes(result);
      return result;
    },
    getLicenses: async () => {
      const result = await licenses.getLicenses();
      store.storeLicenses(result);
      return result;
    },
    getMediaTypes: async () => {
      const result = await mediaTypes.getMediaTypes();
      store.storeMediaTypes(result);
      return result;
    },
    getSeries: async () => {
      const result = await series.getSeries();
      store.storeSeries(result);
      return result;
    },
    getTags: async () => {
      const result = await tags.getTags();
      store.storeTags(result);
      return result;
    },
    getTonePools: async () => {
      const result = await tonePools.getTonePools();
      store.storeTonePools(result);
      return result;
    },
    getUsers: async () => {
      const result = await users.getUsers();
      store.storeUsers(result);
      return result;
    },
  }).current;

  React.useEffect(() => {
    // Initialize the first time the hook is called only.
    // Failures will have to manually be resolved.
    if (!state.actions.length) controller.getActions();
    if (!state.sourceActions.length) controller.getSourceActions();
    if (!state.sourceMetrics.length) controller.getSourceMetrics();
    if (!state.categories.length) controller.getCategories();
    if (!state.contentTypes.length) controller.getContentTypes();
    if (!state.licenses.length) controller.getLicenses();
    if (!state.mediaTypes.length) controller.getMediaTypes();
    if (!state.series.length) controller.getSeries();
    if (!state.tags.length) controller.getTags();
    if (!state.tonePools.length) controller.getTonePools();
    if (!state.users.length) controller.getUsers();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  return [state, controller];
};
