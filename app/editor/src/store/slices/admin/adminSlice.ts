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
  IMediaTypeModel,
  IMinisterModel,
  INotificationModel,
  INotificationTemplateModel,
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
  IUserFilter,
  IUserModel,
  IWorkOrderModel,
} from 'tno-core';

import { IAdminState } from './interfaces';

export const initialAdminState: IAdminState = {
  actionFilter: '',
  actions: [],
  chartTemplates: [],
  connectionFilter: '',
  connections: [],
  contributorFilter: '',
  contributors: [],
  dataLocationFilter: '',
  dataLocations: [],
  folderFilter: '',
  folders: [],
  filterFilter: '',
  filters: [],
  ingestFilter: '',
  ingests: [],
  ingestTypeFilter: '',
  ingestTypes: [],
  licenseFilter: '',
  licenses: [],
  mediaTypeFilter: '',
  mediaTypes: [],
  ministerFilter: '',
  ministers: [],
  organizationFilter: '',
  organizations: [],
  productFilter: '',
  products: [],
  notificationFilter: '',
  notifications: [],
  notificationTemplates: [],
  reportFilter: '',
  reportSubscriberFilter: {},
  reports: [],
  reportTemplates: [],
  rules: [],
  seriesFilter: '',
  series: [],
  sourceFilter: '',
  sources: [],
  systemMessages: [],
  tagFilter: '',
  tags: [],
  topicFilter: '',
  topics: [],
  userFilter: {
    page: 0,
    quantity: 20,
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
    storeActionFilter(state: IAdminState, action: PayloadAction<string>) {
      state.actionFilter = action.payload;
    },
    storeActions(state: IAdminState, action: PayloadAction<IActionModel[]>) {
      state.actions = action.payload;
    },
    storeChartTemplates(state: IAdminState, action: PayloadAction<IChartTemplateModel[]>) {
      state.chartTemplates = action.payload;
    },
    storeConnectionFilter(state: IAdminState, action: PayloadAction<string>) {
      state.connectionFilter = action.payload;
    },
    storeConnections(state: IAdminState, action: PayloadAction<IConnectionModel[]>) {
      state.connections = action.payload;
    },
    storeContributorFilter(state: IAdminState, action: PayloadAction<string>) {
      state.contributorFilter = action.payload;
    },
    storeContributors(state: IAdminState, action: PayloadAction<IContributorModel[]>) {
      state.contributors = action.payload;
    },
    storeDataLocationFilter(state: IAdminState, action: PayloadAction<string>) {
      state.dataLocationFilter = action.payload;
    },
    storeDataLocations(state: IAdminState, action: PayloadAction<IDataLocationModel[]>) {
      state.dataLocations = action.payload;
    },
    storeFolderFilter(state: IAdminState, action: PayloadAction<string>) {
      state.folderFilter = action.payload;
    },
    storeFolders(state: IAdminState, action: PayloadAction<IFolderModel[]>) {
      state.folders = action.payload;
    },
    storeFilterFilter(state: IAdminState, action: PayloadAction<string>) {
      state.filterFilter = action.payload;
    },
    storeFilters(state: IAdminState, action: PayloadAction<IFilterModel[]>) {
      state.filters = action.payload;
    },
    storeIngestFilter(state: IAdminState, action: PayloadAction<string>) {
      state.ingestFilter = action.payload;
    },
    storeIngests(state: IAdminState, action: PayloadAction<IIngestModel[]>) {
      state.ingests = action.payload;
    },
    storeIngestTypeFilter(state: IAdminState, action: PayloadAction<string>) {
      state.ingestTypeFilter = action.payload;
    },
    storeIngestTypes(state: IAdminState, action: PayloadAction<IIngestTypeModel[]>) {
      state.ingestTypes = action.payload;
    },
    storeLicenseFilter(state: IAdminState, action: PayloadAction<string>) {
      state.licenseFilter = action.payload;
    },
    storeLicenses(state: IAdminState, action: PayloadAction<ILicenseModel[]>) {
      state.licenses = action.payload;
    },
    storeMediaTypeFilter(state: IAdminState, action: PayloadAction<string>) {
      state.mediaTypeFilter = action.payload;
    },
    storeMediaTypes(state: IAdminState, action: PayloadAction<IMediaTypeModel[]>) {
      state.mediaTypes = action.payload;
    },
    storeMinisterFilter(state: IAdminState, action: PayloadAction<string>) {
      state.ministerFilter = action.payload;
    },
    storeMinisters(state: IAdminState, action: PayloadAction<IMinisterModel[]>) {
      state.ministers = action.payload;
    },
    storeNotificationFilter(state: IAdminState, action: PayloadAction<string>) {
      state.notificationFilter = action.payload;
    },
    storeNotifications(state: IAdminState, action: PayloadAction<INotificationModel[]>) {
      state.notifications = action.payload;
    },
    storeNotificationTemplates(
      state: IAdminState,
      action: PayloadAction<INotificationTemplateModel[]>,
    ) {
      state.notificationTemplates = action.payload;
    },
    storeOrganizationFilter(state: IAdminState, action: PayloadAction<string>) {
      state.organizationFilter = action.payload;
    },
    storeOrganizations(state: IAdminState, action: PayloadAction<IOrganizationModel[]>) {
      state.organizations = action.payload;
    },
    storeProductFilter(state: IAdminState, action: PayloadAction<string>) {
      state.productFilter = action.payload;
    },
    storeProducts(state: IAdminState, action: PayloadAction<IProductModel[]>) {
      state.products = action.payload;
    },
    storeReportFilter(state: IAdminState, action: PayloadAction<string>) {
      state.reportFilter = action.payload;
    },
    storeReportSubscriberFilter(state: IAdminState, action: PayloadAction<IUserFilter>) {
      state.reportSubscriberFilter = action.payload;
    },
    storeReports(state: IAdminState, action: PayloadAction<IReportModel[]>) {
      state.reports = action.payload;
    },
    storeReportTemplates(state: IAdminState, action: PayloadAction<IReportTemplateModel[]>) {
      state.reportTemplates = action.payload;
    },
    storeSeriesFilter(state: IAdminState, action: PayloadAction<string>) {
      state.seriesFilter = action.payload;
    },
    storeSeries(state: IAdminState, action: PayloadAction<ISeriesModel[]>) {
      state.series = action.payload;
    },
    storeSourceFilter(state: IAdminState, action: PayloadAction<string>) {
      state.sourceFilter = action.payload;
    },
    storeSources(state: IAdminState, action: PayloadAction<ISourceModel[]>) {
      state.sources = action.payload;
    },
    storeSystemMessages(state: IAdminState, action: PayloadAction<ISystemMessageModel[]>) {
      state.systemMessages = action.payload;
    },
    storeTagFilter(state: IAdminState, action: PayloadAction<string>) {
      state.tagFilter = action.payload;
    },
    storeTags(state: IAdminState, action: PayloadAction<ITagModel[]>) {
      state.tags = action.payload;
    },
    storeTopicFilter(state: IAdminState, action: PayloadAction<string>) {
      state.topicFilter = action.payload;
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
  storeActionFilter: storeAdminActionFilter,
  storeActions: storeAdminActions,
  storeChartTemplates: storeAdminChartTemplates,
  storeConnectionFilter: storeAdminConnectionFilter,
  storeConnections: storeAdminConnections,
  storeContributorFilter: storeAdminContributorFilter,
  storeContributors: storeAdminContributors,
  storeDataLocationFilter: storeAdminDataLocationFilter,
  storeDataLocations: storeAdminDataLocations,
  storeFolderFilter: storeAdminFolderFilter,
  storeFolders: storeAdminFolders,
  storeFilterFilter: storeAdminFilterFilter,
  storeFilters: storeAdminFilters,
  storeIngestFilter: storeAdminIngestFilter,
  storeIngests: storeAdminIngests,
  storeIngestTypeFilter: storeAdminIngestTypeFilter,
  storeIngestTypes: storeAdminIngestTypes,
  storeLicenseFilter: storeAdminLicenseFilter,
  storeLicenses: storeAdminLicenses,
  storeMediaTypeFilter: storeAdminMediaTypeFilter,
  storeMediaTypes: storeAdminMediaTypes,
  storeMinisterFilter: storeAdminMinisterFilter,
  storeMinisters: storeAdminMinisters,
  storeNotificationFilter: storeAdminNotificationFilter,
  storeNotifications: storeAdminNotifications,
  storeNotificationTemplates: storeAdminNotificationTemplates,
  storeOrganizationFilter: storeAdminOrganizationFilter,
  storeOrganizations: storeAdminOrganizations,
  storeProductFilter: storeAdminProductFilter,
  storeProducts: storeAdminProducts,
  storeReportFilter: storeAdminReportFilter,
  storeReportSubscriberFilter: storeAdminReportSubscriberFilter,
  storeReports: storeAdminReports,
  storeReportTemplates: storeAdminReportTemplates,
  storeSeriesFilter: storeAdminSeriesFilter,
  storeSeries: storeAdminSeries,
  storeSourceFilter: storeAdminSourceFilter,
  storeSources: storeAdminSources,
  storeSystemMessages: storeAdminSystemMessages,
  storeTagFilter: storeAdminTagFilter,
  storeTags: storeAdminTags,
  storeTopicFilter: storeAdminTopicFilter,
  storeTopics: storeAdminTopics,
  storeTopicScoreRules: storeAdminTopicScoreRules,
  storeUserFilter: storeAdminUserFilter,
  storeUsers: storeAdminUsers,
  storeWorkOrderFilter: storeAdminWorkOrderFilter,
  storeWorkOrders: storeAdminWorkOrders,
  storeSettings: storeAdminSettings,
} = adminSlice.actions;
