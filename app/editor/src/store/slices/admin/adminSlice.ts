import { createSlice, PayloadAction } from '@reduxjs/toolkit';
import { IUserListFilter } from 'features/admin/users/interfaces/IUserListFilter';
import { IWorkOrderListFilter } from 'features/admin/work-orders/interfaces/IWorkOrderListFilter';
import {
  IActionModel,
  IConnectionModel,
  IContributorModel,
  IDataLocationModel,
  IIngestModel,
  IIngestTypeModel,
  ILicenseModel,
  INotificationModel,
  IPaged,
  IProductModel,
  IReportModel,
  IReportTemplateModel,
  ISeriesModel,
  ISourceModel,
  ISystemMessageModel,
  ITagModel,
  ITopicModel,
  ITopicScoreRuleModel,
  IUserModel,
  IWorkOrderModel,
} from 'tno-core';

import { IAdminState } from './interfaces';

export const initialAdminState: IAdminState = {
  sources: [],
  connections: [],
  dataLocations: [],
  products: [],
  ingests: [],
  ingestTypes: [],
  userFilter: {
    pageIndex: 0,
    pageSize: 20,
    sort: [],
  },
  users: { page: 1, quantity: 20, items: [], total: 0 },
  topics: [],
  rules: [],
  tags: [],
  systemMessages: [],
  actions: [],
  series: [],
  contributors: [],
  licenses: [],
  workOrderFilter: {
    pageIndex: 0,
    pageSize: 10,
    status: '',
    workType: '',
    keyword: '',
    sort: [],
  },
  workOrders: { page: 1, quantity: 10, items: [], total: 0 },
  reports: [],
  reportTemplates: [],
  notifications: [],
};

export const adminSlice = createSlice({
  name: 'admin',
  initialState: initialAdminState,
  reducers: {
    storeSources(state: IAdminState, action: PayloadAction<ISourceModel[]>) {
      state.sources = action.payload;
    },
    storeIngests(state: IAdminState, action: PayloadAction<IIngestModel[]>) {
      state.ingests = action.payload;
    },
    storeConnections(state: IAdminState, action: PayloadAction<IConnectionModel[]>) {
      state.connections = action.payload;
    },
    storeDataLocations(state: IAdminState, action: PayloadAction<IDataLocationModel[]>) {
      state.dataLocations = action.payload;
    },
    storeProducts(state: IAdminState, action: PayloadAction<IProductModel[]>) {
      state.products = action.payload;
    },
    storeLicenses(state: IAdminState, action: PayloadAction<ILicenseModel[]>) {
      state.licenses = action.payload;
    },
    storeIngestTypes(state: IAdminState, action: PayloadAction<IIngestTypeModel[]>) {
      state.ingestTypes = action.payload;
    },
    storeUserFilter(state: IAdminState, action: PayloadAction<IUserListFilter>) {
      state.userFilter = action.payload;
    },
    storeUsers(state: IAdminState, action: PayloadAction<IPaged<IUserModel>>) {
      state.users = action.payload;
    },
    storeTopics(state: IAdminState, action: PayloadAction<ITopicModel[]>) {
      state.topics = action.payload;
    },
    storeTopicScoreRules(state: IAdminState, action: PayloadAction<ITopicScoreRuleModel[]>) {
      state.rules = action.payload;
    },
    storeTags(state: IAdminState, action: PayloadAction<ITagModel[]>) {
      state.tags = action.payload;
    },
    storeSystemMessages(state: IAdminState, action: PayloadAction<ISystemMessageModel[]>) {
      state.systemMessages = action.payload;
    },
    storeActions(state: IAdminState, action: PayloadAction<IActionModel[]>) {
      state.actions = action.payload;
    },
    storeSeries(state: IAdminState, action: PayloadAction<ISeriesModel[]>) {
      state.series = action.payload;
    },
    storeContributors(state: IAdminState, action: PayloadAction<IContributorModel[]>) {
      state.contributors = action.payload;
    },
    storeWorkOrderFilter(state: IAdminState, action: PayloadAction<IWorkOrderListFilter>) {
      state.workOrderFilter = action.payload;
    },
    storeWorkOrders(state: IAdminState, action: PayloadAction<IPaged<IWorkOrderModel>>) {
      state.workOrders = action.payload;
    },
    storeReports(state: IAdminState, action: PayloadAction<IReportModel[]>) {
      state.reports = action.payload;
    },
    storeReportTemplates(state: IAdminState, action: PayloadAction<IReportTemplateModel[]>) {
      state.reportTemplates = action.payload;
    },
    storeNotifications(state: IAdminState, action: PayloadAction<INotificationModel[]>) {
      state.notifications = action.payload;
    },
  },
});

export const {
  storeSources: storeAdminSources,
  storeConnections: storeAdminConnections,
  storeDataLocations: storeAdminDataLocations,
  storeProducts: storeAdminProducts,
  storeLicenses: storeAdminLicenses,
  storeIngests: storeAdminIngests,
  storeIngestTypes: storeAdminIngestTypes,
  storeUserFilter: storeAdminUserFilter,
  storeUsers: storeAdminUsers,
  storeTopics: storeAdminTopics,
  storeTopicScoreRules: storeAdminTopicScoreRules,
  storeTags: storeAdminTags,
  storeSystemMessages: storeAdminSystemMessages,
  storeActions: storeAdminActions,
  storeSeries: storeAdminSeries,
  storeContributors: storeAdminContributors,
  storeWorkOrderFilter: storeAdminWorkOrderFilter,
  storeWorkOrders: storeAdminWorkOrders,
  storeReports: storeAdminReports,
  storeReportTemplates: storeAdminReportTemplates,
  storeNotifications: storeAdminNotifications,
} = adminSlice.actions;
