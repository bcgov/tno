import { createSlice, PayloadAction } from '@reduxjs/toolkit';
import { IUserListFilter } from 'features/admin/users/interfaces/IUserListFilter';
import { IWorkOrderListFilter } from 'features/admin/work-orders/interfaces/IWorkOrderListFilter';
import {
  IActionModel,
  IChartTemplateModel,
  IConnectionModel,
  IContributorModel,
  IDataLocationModel,
  IFilterModel,
  IFolderModel,
  IIngestModel,
  IIngestTypeModel,
  ILicenseModel,
  IMinisterModel,
  INotificationModel,
  IOrganizationModel,
  IPaged,
  IProductModel,
  IReportModel,
  IReportTemplateModel,
  ISeriesModel,
  ISettingModel,
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
  actions: [],
  chartTemplates: [],
  connections: [],
  contributors: [],
  dataLocations: [],
  folders: [],
  filters: [],
  ingests: [],
  ingestTypes: [],
  licenses: [],
  ministers: [],
  organizations: [],
  notifications: [],
  products: [],
  reports: [],
  reportTemplates: [],
  rules: [],
  series: [],
  sources: [],
  systemMessages: [],
  tags: [],
  topics: [],
  userFilter: {
    pageIndex: 0,
    pageSize: 20,
    sort: [],
  },
  users: { page: 1, quantity: 20, items: [], total: 0 },
  workOrderFilter: {
    pageIndex: 0,
    pageSize: 10,
    status: '',
    workType: '',
    keyword: '',
    sort: [],
  },
  workOrders: { page: 1, quantity: 10, items: [], total: 0 },
  settings: [],
};

export const adminSlice = createSlice({
  name: 'admin',
  initialState: initialAdminState,
  reducers: {
    storeActions(state: IAdminState, action: PayloadAction<IActionModel[]>) {
      state.actions = action.payload;
    },
    storeChartTemplates(state: IAdminState, action: PayloadAction<IChartTemplateModel[]>) {
      state.chartTemplates = action.payload;
    },
    storeConnections(state: IAdminState, action: PayloadAction<IConnectionModel[]>) {
      state.connections = action.payload;
    },
    storeContributors(state: IAdminState, action: PayloadAction<IContributorModel[]>) {
      state.contributors = action.payload;
    },
    storeDataLocations(state: IAdminState, action: PayloadAction<IDataLocationModel[]>) {
      state.dataLocations = action.payload;
    },
    storeFolders(state: IAdminState, action: PayloadAction<IFolderModel[]>) {
      state.folders = action.payload;
    },
    storeFilters(state: IAdminState, action: PayloadAction<IFilterModel[]>) {
      state.filters = action.payload;
    },
    storeIngests(state: IAdminState, action: PayloadAction<IIngestModel[]>) {
      state.ingests = action.payload;
    },
    storeIngestTypes(state: IAdminState, action: PayloadAction<IIngestTypeModel[]>) {
      state.ingestTypes = action.payload;
    },
    storeLicenses(state: IAdminState, action: PayloadAction<ILicenseModel[]>) {
      state.licenses = action.payload;
    },
    storeMinisters(state: IAdminState, action: PayloadAction<IMinisterModel[]>) {
      state.ministers = action.payload;
    },
    storeNotifications(state: IAdminState, action: PayloadAction<INotificationModel[]>) {
      state.notifications = action.payload;
    },
    storeOrganizations(state: IAdminState, action: PayloadAction<IOrganizationModel[]>) {
      state.organizations = action.payload;
    },
    storeProducts(state: IAdminState, action: PayloadAction<IProductModel[]>) {
      state.products = action.payload;
    },
    storeReports(state: IAdminState, action: PayloadAction<IReportModel[]>) {
      state.reports = action.payload;
    },
    storeReportTemplates(state: IAdminState, action: PayloadAction<IReportTemplateModel[]>) {
      state.reportTemplates = action.payload;
    },
    storeSeries(state: IAdminState, action: PayloadAction<ISeriesModel[]>) {
      state.series = action.payload;
    },
    storeSources(state: IAdminState, action: PayloadAction<ISourceModel[]>) {
      state.sources = action.payload;
    },
    storeSystemMessages(state: IAdminState, action: PayloadAction<ISystemMessageModel[]>) {
      state.systemMessages = action.payload;
    },
    storeTags(state: IAdminState, action: PayloadAction<ITagModel[]>) {
      state.tags = action.payload;
    },
    storeTopics(state: IAdminState, action: PayloadAction<ITopicModel[]>) {
      state.topics = action.payload;
    },
    storeTopicScoreRules(state: IAdminState, action: PayloadAction<ITopicScoreRuleModel[]>) {
      state.rules = action.payload;
    },
    storeUserFilter(state: IAdminState, action: PayloadAction<IUserListFilter>) {
      state.userFilter = action.payload;
    },
    storeUsers(state: IAdminState, action: PayloadAction<IPaged<IUserModel>>) {
      state.users = action.payload;
    },
    storeWorkOrderFilter(state: IAdminState, action: PayloadAction<IWorkOrderListFilter>) {
      state.workOrderFilter = action.payload;
    },
    storeWorkOrders(state: IAdminState, action: PayloadAction<IPaged<IWorkOrderModel>>) {
      state.workOrders = action.payload;
    },
    storeSettings(state: IAdminState, action: PayloadAction<ISettingModel[]>) {
      state.settings = action.payload;
    },
  },
});

export const {
  storeActions: storeAdminActions,
  storeChartTemplates: storeAdminChartTemplates,
  storeConnections: storeAdminConnections,
  storeContributors: storeAdminContributors,
  storeDataLocations: storeAdminDataLocations,
  storeFolders: storeAdminFolders,
  storeFilters: storeAdminFilters,
  storeIngests: storeAdminIngests,
  storeIngestTypes: storeAdminIngestTypes,
  storeLicenses: storeAdminLicenses,
  storeMinisters: storeAdminMinisters,
  storeNotifications: storeAdminNotifications,
  storeOrganizations: storeAdminOrganizations,
  storeProducts: storeAdminProducts,
  storeReports: storeAdminReports,
  storeReportTemplates: storeAdminReportTemplates,
  storeSeries: storeAdminSeries,
  storeSources: storeAdminSources,
  storeSystemMessages: storeAdminSystemMessages,
  storeTags: storeAdminTags,
  storeTopics: storeAdminTopics,
  storeTopicScoreRules: storeAdminTopicScoreRules,
  storeUserFilter: storeAdminUserFilter,
  storeUsers: storeAdminUsers,
  storeWorkOrderFilter: storeAdminWorkOrderFilter,
  storeWorkOrders: storeAdminWorkOrders,
  storeSettings: storeAdminSettings,
} = adminSlice.actions;
