import { IUserListFilter } from 'features/admin/users/interfaces/IUserListFilter';
import { IWorkOrderListFilter } from 'features/admin/work-orders/interfaces/IWorkOrderListFilter';
import React from 'react';
import { ActionDelegate, useAppDispatch, useAppSelector } from 'store';
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

import {
  storeAdminActionFilter,
  storeAdminActions,
  storeAdminChartTemplates,
  storeAdminConnectionFilter,
  storeAdminConnections,
  storeAdminContributorFilter,
  storeAdminContributors,
  storeAdminDataLocationFilter,
  storeAdminDataLocations,
  storeAdminFilterFilter,
  storeAdminFilters,
  storeAdminFolderFilter,
  storeAdminFolders,
  storeAdminIngestFilter,
  storeAdminIngests,
  storeAdminIngestTypeFilter,
  storeAdminIngestTypes,
  storeAdminLicenseFilter,
  storeAdminLicenses,
  storeAdminMediaTypeFilter,
  storeAdminMediaTypes,
  storeAdminMinisterFilter,
  storeAdminMinisters,
  storeAdminNotificationFilter,
  storeAdminNotifications,
  storeAdminNotificationTemplates,
  storeAdminOrganizationFilter,
  storeAdminOrganizations,
  storeAdminProductFilter,
  storeAdminProducts,
  storeAdminReportFilter,
  storeAdminReports,
  storeAdminReportSubscriberFilter,
  storeAdminReportTemplates,
  storeAdminSeries,
  storeAdminSeriesFilter,
  storeAdminSettings,
  storeAdminSourceFilter,
  storeAdminSources,
  storeAdminSystemMessages,
  storeAdminTagFilter,
  storeAdminTags,
  storeAdminTopicFilter,
  storeAdminTopics,
  storeAdminTopicScoreRules,
  storeAdminUserFilter,
  storeAdminUsers,
  storeAdminWorkOrderFilter,
  storeAdminWorkOrders,
} from '.';
import { IAdminState } from './interfaces';

export interface IAdminProps {}

export interface IAdminStore {
  storeActionFilter: (filter: string | ActionDelegate<string>) => void;
  storeActions: (actions: IActionModel[] | ActionDelegate<IActionModel[]>) => void;
  storeChartTemplates: (
    chartTemplates: IChartTemplateModel[] | ActionDelegate<IChartTemplateModel[]>,
  ) => void;
  storeConnectionFilter: (filter: string | ActionDelegate<string>) => void;
  storeConnections: (connections: IConnectionModel[] | ActionDelegate<IConnectionModel[]>) => void;
  storeContributorFilter: (filter: string | ActionDelegate<string>) => void;
  storeContributors: (series: IContributorModel[] | ActionDelegate<IContributorModel[]>) => void;
  storeDataLocationFilter: (filter: string | ActionDelegate<string>) => void;
  storeDataLocations: (
    dataLocations: IDataLocationModel[] | ActionDelegate<IDataLocationModel[]>,
  ) => void;
  storeFolderFilter: (filter: string | ActionDelegate<string>) => void;
  storeFolders: (folders: IFolderModel[] | ActionDelegate<IFolderModel[]>) => void;
  storeFilterFilter: (filter: string | ActionDelegate<string>) => void;
  storeFilters: (filters: IFilterModel[] | ActionDelegate<IFilterModel[]>) => void;
  storeIngestFilter: (filter: string | ActionDelegate<string>) => void;
  storeIngests: (ingests: IIngestModel[] | ActionDelegate<IIngestModel[]>) => void;
  storeIngestTypeFilter: (filter: string | ActionDelegate<string>) => void;
  storeIngestTypes: (ingestTypes: IIngestTypeModel[] | ActionDelegate<IIngestTypeModel[]>) => void;
  storeLicenseFilter: (filter: string | ActionDelegate<string>) => void;
  storeLicenses: (licenses: ILicenseModel[] | ActionDelegate<ILicenseModel[]>) => void;
  storeMediaTypeFilter: (filter: string | ActionDelegate<string>) => void;
  storeMediaTypes: (mediaTypes: IMediaTypeModel[] | ActionDelegate<IMediaTypeModel[]>) => void;
  storeMinisterFilter: (filter: string | ActionDelegate<string>) => void;
  storeMinisters: (ministers: IMinisterModel[] | ActionDelegate<IMinisterModel[]>) => void;
  storeNotificationFilter: (filter: string | ActionDelegate<string>) => void;
  storeNotifications: (
    notifications: INotificationModel[] | ActionDelegate<INotificationModel[]>,
  ) => void;
  storeNotificationTemplates: (
    templates: INotificationTemplateModel[] | ActionDelegate<INotificationTemplateModel[]>,
  ) => void;
  storeOrganizationFilter: (filter: string | ActionDelegate<string>) => void;
  storeOrganizations: (
    organizations: IOrganizationModel[] | ActionDelegate<IOrganizationModel[]>,
  ) => void;
  storeProductFilter: (filter: string | ActionDelegate<string>) => void;
  storeProducts: (products: IProductModel[] | ActionDelegate<IProductModel[]>) => void;
  storeReportFilter: (filter: string | ActionDelegate<string>) => void;
  storeReportSubscriberFilter: (filter: IUserFilter | ActionDelegate<IUserFilter>) => void;
  storeReports: (reports: IReportModel[] | ActionDelegate<IReportModel[]>) => void;
  storeReportTemplates: (
    reportTemplates: IReportTemplateModel[] | ActionDelegate<IReportTemplateModel[]>,
  ) => void;
  storeSeriesFilter: (filter: string | ActionDelegate<string>) => void;
  storeSeries: (series: ISeriesModel[] | ActionDelegate<ISeriesModel[]>) => void;
  storeSourceFilter: (filter: string | ActionDelegate<string>) => void;
  storeSources: (sources: ISourceModel[] | ActionDelegate<ISourceModel[]>) => void;
  storeSystemMessages: (
    systemMessages: ISystemMessageModel[] | ActionDelegate<ISystemMessageModel[]>,
  ) => void;
  storeTagFilter: (filter: string | ActionDelegate<string>) => void;
  storeTags: (tags: ITagModel[] | ActionDelegate<ITagModel[]>) => void;
  storeTopicFilter: (filter: string | ActionDelegate<string>) => void;
  storeTopics: (topics: ITopicModel[] | ActionDelegate<ITopicModel[]>) => void;
  storeTopicScoreRules: (
    rules: ITopicScoreRuleModel[] | ActionDelegate<ITopicScoreRuleModel[]>,
  ) => void;
  storeUserFilter: (filter: IUserListFilter | ActionDelegate<IUserListFilter>) => void;
  storeUsers: (users: IPaged<IUserModel> | ActionDelegate<IPaged<IUserModel>>) => void;
  storeWorkOrderFilter: (
    filter: IWorkOrderListFilter | ActionDelegate<IWorkOrderListFilter>,
  ) => void;
  storeWorkOrders: (
    workOrders: IPaged<IWorkOrderModel> | ActionDelegate<IPaged<IWorkOrderModel>>,
  ) => void;
  storeSettings: (settings: ISettingModel[] | ActionDelegate<ISettingModel[]>) => void;
}

export const useAdminStore = (props?: IAdminProps): [IAdminState, IAdminStore] => {
  const dispatch = useAppDispatch();
  const state = useAppSelector((store) => store.admin);

  const controller = React.useMemo(
    () => ({
      storeActionFilter: (filter: string | ActionDelegate<string>) => {
        if (typeof filter === 'function') {
          dispatch(storeAdminActionFilter(filter(state.actionFilter)));
        } else dispatch(storeAdminActionFilter(filter));
      },
      storeActions: (actions: IActionModel[] | ActionDelegate<IActionModel[]>) => {
        if (typeof actions === 'function') {
          dispatch(storeAdminActions(actions(state.actions)));
        } else dispatch(storeAdminActions(actions));
      },
      storeChartTemplates: (
        chartTemplates: IChartTemplateModel[] | ActionDelegate<IChartTemplateModel[]>,
      ) => {
        if (typeof chartTemplates === 'function') {
          dispatch(storeAdminChartTemplates(chartTemplates(state.chartTemplates)));
        } else dispatch(storeAdminChartTemplates(chartTemplates));
      },
      storeConnectionFilter: (filter: string | ActionDelegate<string>) => {
        if (typeof filter === 'function') {
          dispatch(storeAdminConnectionFilter(filter(state.connectionFilter)));
        } else dispatch(storeAdminConnectionFilter(filter));
      },
      storeConnections: (connections: IConnectionModel[] | ActionDelegate<IConnectionModel[]>) => {
        if (typeof connections === 'function') {
          dispatch(storeAdminConnections(connections(state.connections)));
        } else dispatch(storeAdminConnections(connections));
      },
      storeContributorFilter: (filter: string | ActionDelegate<string>) => {
        if (typeof filter === 'function') {
          dispatch(storeAdminContributorFilter(filter(state.contributorFilter)));
        } else dispatch(storeAdminContributorFilter(filter));
      },
      storeContributors: (
        contributors: IContributorModel[] | ActionDelegate<IContributorModel[]>,
      ) => {
        if (typeof contributors === 'function') {
          dispatch(storeAdminContributors(contributors(state.contributors)));
        } else dispatch(storeAdminContributors(contributors));
      },
      storeDataLocationFilter: (filter: string | ActionDelegate<string>) => {
        if (typeof filter === 'function') {
          dispatch(storeAdminDataLocationFilter(filter(state.dataLocationFilter)));
        } else dispatch(storeAdminDataLocationFilter(filter));
      },
      storeDataLocations: (
        dataLocations: IDataLocationModel[] | ActionDelegate<IDataLocationModel[]>,
      ) => {
        if (typeof dataLocations === 'function') {
          dispatch(storeAdminDataLocations(dataLocations(state.dataLocations)));
        } else dispatch(storeAdminDataLocations(dataLocations));
      },
      storeFilterFilter: (filter: string | ActionDelegate<string>) => {
        if (typeof filter === 'function') {
          dispatch(storeAdminFilterFilter(filter(state.filterFilter)));
        } else dispatch(storeAdminFilterFilter(filter));
      },
      storeFilters: (filters: IFilterModel[] | ActionDelegate<IFilterModel[]>) => {
        if (typeof filters === 'function') {
          dispatch(storeAdminFilters(filters(state.filters)));
        } else dispatch(storeAdminFilters(filters));
      },
      storeFolderFilter: (filter: string | ActionDelegate<string>) => {
        if (typeof filter === 'function') {
          dispatch(storeAdminFolderFilter(filter(state.folderFilter)));
        } else dispatch(storeAdminFolderFilter(filter));
      },
      storeFolders: (folders: IFolderModel[] | ActionDelegate<IFolderModel[]>) => {
        if (typeof folders === 'function') {
          dispatch(storeAdminFolders(folders(state.folders)));
        } else dispatch(storeAdminFolders(folders));
      },
      storeIngestFilter: (filter: string | ActionDelegate<string>) => {
        if (typeof filter === 'function') {
          dispatch(storeAdminIngestFilter(filter(state.ingestFilter)));
        } else dispatch(storeAdminIngestFilter(filter));
      },
      storeIngests: (ingests: IIngestModel[] | ActionDelegate<IIngestModel[]>) => {
        if (typeof ingests === 'function') {
          dispatch(storeAdminIngests(ingests(state.ingests)));
        } else dispatch(storeAdminIngests(ingests));
      },
      storeIngestTypeFilter: (filter: string | ActionDelegate<string>) => {
        if (typeof filter === 'function') {
          dispatch(storeAdminIngestTypeFilter(filter(state.ingestTypeFilter)));
        } else dispatch(storeAdminIngestTypeFilter(filter));
      },
      storeIngestTypes: (ingestTypes: IIngestTypeModel[] | ActionDelegate<IIngestTypeModel[]>) => {
        if (typeof ingestTypes === 'function') {
          dispatch(storeAdminIngestTypes(ingestTypes(state.ingestTypes)));
        } else dispatch(storeAdminIngestTypes(ingestTypes));
      },
      storeLicenseFilter: (filter: string | ActionDelegate<string>) => {
        if (typeof filter === 'function') {
          dispatch(storeAdminLicenseFilter(filter(state.licenseFilter)));
        } else dispatch(storeAdminLicenseFilter(filter));
      },
      storeLicenses: (licenses: ILicenseModel[] | ActionDelegate<ILicenseModel[]>) => {
        if (typeof licenses === 'function') {
          dispatch(storeAdminLicenses(licenses(state.licenses)));
        } else dispatch(storeAdminLicenses(licenses));
      },
      storeMinisterFilter: (filter: string | ActionDelegate<string>) => {
        if (typeof filter === 'function') {
          dispatch(storeAdminMinisterFilter(filter(state.ministerFilter)));
        } else dispatch(storeAdminMinisterFilter(filter));
      },
      storeMinisters: (ministers: IMinisterModel[] | ActionDelegate<IMinisterModel[]>) => {
        if (typeof ministers === 'function') {
          dispatch(storeAdminMinisters(ministers(state.ministers)));
        } else dispatch(storeAdminMinisters(ministers));
      },
      storeNotificationFilter: (filter: string | ActionDelegate<string>) => {
        if (typeof filter === 'function') {
          dispatch(storeAdminNotificationFilter(filter(state.notificationFilter)));
        } else dispatch(storeAdminNotificationFilter(filter));
      },
      storeNotifications: (
        notifications: INotificationModel[] | ActionDelegate<INotificationModel[]>,
      ) => {
        if (typeof notifications === 'function') {
          dispatch(storeAdminNotifications(notifications(state.notifications)));
        } else dispatch(storeAdminNotifications(notifications));
      },
      storeNotificationTemplates: (
        templates: INotificationTemplateModel[] | ActionDelegate<INotificationTemplateModel[]>,
      ) => {
        if (typeof templates === 'function') {
          dispatch(storeAdminNotificationTemplates(templates(state.notificationTemplates)));
        } else dispatch(storeAdminNotificationTemplates(templates));
      },
      storeOrganizationFilter: (filter: string | ActionDelegate<string>) => {
        if (typeof filter === 'function') {
          dispatch(storeAdminOrganizationFilter(filter(state.organizationFilter)));
        } else dispatch(storeAdminOrganizationFilter(filter));
      },
      storeOrganizations: (
        organizations: IOrganizationModel[] | ActionDelegate<IOrganizationModel[]>,
      ) => {
        if (typeof organizations === 'function') {
          dispatch(storeAdminOrganizations(organizations(state.organizations)));
        } else dispatch(storeAdminOrganizations(organizations));
      },
      storeMediaTypeFilter: (filter: string | ActionDelegate<string>) => {
        if (typeof filter === 'function') {
          dispatch(storeAdminMediaTypeFilter(filter(state.mediaTypeFilter)));
        } else dispatch(storeAdminMediaTypeFilter(filter));
      },
      storeMediaTypes: (mediaTypes: IMediaTypeModel[] | ActionDelegate<IMediaTypeModel[]>) => {
        if (typeof mediaTypes === 'function') {
          dispatch(storeAdminMediaTypes(mediaTypes(state.mediaTypes)));
        } else dispatch(storeAdminMediaTypes(mediaTypes));
      },
      storeProductFilter: (filter: string | ActionDelegate<string>) => {
        if (typeof filter === 'function') {
          dispatch(storeAdminProductFilter(filter(state.productFilter)));
        } else dispatch(storeAdminProductFilter(filter));
      },
      storeProducts: (products: IProductModel[] | ActionDelegate<IProductModel[]>) => {
        if (typeof products === 'function') {
          dispatch(storeAdminProducts(products(state.products)));
        } else dispatch(storeAdminProducts(products));
      },
      storeReportFilter: (filter: string | ActionDelegate<string>) => {
        if (typeof filter === 'function') {
          dispatch(storeAdminReportFilter(filter(state.reportFilter)));
        } else dispatch(storeAdminReportFilter(filter));
      },
      storeReportSubscriberFilter: (filter: IUserFilter | ActionDelegate<IUserFilter>) => {
        if (typeof filter === 'function') {
          dispatch(storeAdminReportSubscriberFilter(filter(state.reportSubscriberFilter)));
        } else dispatch(storeAdminReportSubscriberFilter(filter));
      },
      storeReports: (reports: IReportModel[] | ActionDelegate<IReportModel[]>) => {
        if (typeof reports === 'function') {
          dispatch(storeAdminReports(reports(state.reports)));
        } else dispatch(storeAdminReports(reports));
      },
      storeReportTemplates: (
        reportTemplates: IReportTemplateModel[] | ActionDelegate<IReportTemplateModel[]>,
      ) => {
        if (typeof reportTemplates === 'function') {
          dispatch(storeAdminReportTemplates(reportTemplates(state.reportTemplates)));
        } else dispatch(storeAdminReportTemplates(reportTemplates));
      },
      storeSeriesFilter: (filter: string | ActionDelegate<string>) => {
        if (typeof filter === 'function') {
          dispatch(storeAdminSeriesFilter(filter(state.seriesFilter)));
        } else dispatch(storeAdminSeriesFilter(filter));
      },
      storeSeries: (series: ISeriesModel[] | ActionDelegate<ISeriesModel[]>) => {
        if (typeof series === 'function') {
          dispatch(storeAdminSeries(series(state.series)));
        } else dispatch(storeAdminSeries(series));
      },
      storeSourceFilter: (filter: string | ActionDelegate<string>) => {
        if (typeof filter === 'function') {
          dispatch(storeAdminSourceFilter(filter(state.sourceFilter)));
        } else dispatch(storeAdminSourceFilter(filter));
      },
      storeSources: (sources: ISourceModel[] | ActionDelegate<ISourceModel[]>) => {
        if (typeof sources === 'function') {
          dispatch(storeAdminSources(sources(state.sources)));
        } else dispatch(storeAdminSources(sources));
      },
      storeSystemMessages: (
        systemMessages: ISystemMessageModel[] | ActionDelegate<ISystemMessageModel[]>,
      ) => {
        if (typeof systemMessages === 'function') {
          dispatch(storeAdminSystemMessages(systemMessages(state.systemMessages)));
        } else dispatch(storeAdminSystemMessages(systemMessages));
      },
      storeTagFilter: (filter: string | ActionDelegate<string>) => {
        if (typeof filter === 'function') {
          dispatch(storeAdminTagFilter(filter(state.tagFilter)));
        } else dispatch(storeAdminTagFilter(filter));
      },
      storeTags: (tags: ITagModel[] | ActionDelegate<ITagModel[]>) => {
        if (typeof tags === 'function') {
          dispatch(storeAdminTags(tags(state.tags)));
        } else dispatch(storeAdminTags(tags));
      },
      storeTopicFilter: (filter: string | ActionDelegate<string>) => {
        if (typeof filter === 'function') {
          dispatch(storeAdminTopicFilter(filter(state.topicFilter)));
        } else dispatch(storeAdminTopicFilter(filter));
      },
      storeTopics: (topics: ITopicModel[] | ActionDelegate<ITopicModel[]>) => {
        if (typeof topics === 'function') {
          dispatch(storeAdminTopics(topics(state.topics)));
        } else dispatch(storeAdminTopics(topics));
      },
      storeTopicScoreRules: (
        rules: ITopicScoreRuleModel[] | ActionDelegate<ITopicScoreRuleModel[]>,
      ) => {
        if (typeof rules === 'function') {
          dispatch(storeAdminTopicScoreRules(rules(state.rules)));
        } else dispatch(storeAdminTopicScoreRules(rules));
      },
      storeUserFilter: (filter: IUserListFilter | ActionDelegate<IUserListFilter>) => {
        if (typeof filter === 'function') {
          dispatch(storeAdminUserFilter(filter(state.userFilter)));
        } else dispatch(storeAdminUserFilter(filter));
      },
      storeUsers: (users: IPaged<IUserModel> | ActionDelegate<IPaged<IUserModel>>) => {
        if (typeof users === 'function') {
          dispatch(storeAdminUsers(users(state.users)));
        } else dispatch(storeAdminUsers(users));
      },
      storeWorkOrderFilter: (
        filter: IWorkOrderListFilter | ActionDelegate<IWorkOrderListFilter>,
      ) => {
        if (typeof filter === 'function') {
          dispatch(storeAdminWorkOrderFilter(filter(state.workOrderFilter)));
        } else dispatch(storeAdminWorkOrderFilter(filter));
      },
      storeWorkOrders: (
        workOrders: IPaged<IWorkOrderModel> | ActionDelegate<IPaged<IWorkOrderModel>>,
      ) => {
        if (typeof workOrders === 'function') {
          dispatch(storeAdminWorkOrders(workOrders(state.workOrders)));
        } else dispatch(storeAdminWorkOrders(workOrders));
      },
      storeSettings: (settings: ISettingModel[] | ActionDelegate<ISettingModel[]>) => {
        if (typeof settings === 'function') {
          dispatch(storeAdminSettings(settings(state.settings)));
        } else dispatch(storeAdminSettings(settings));
      },
    }),
    [
      dispatch,
      state.actionFilter,
      state.actions,
      state.chartTemplates,
      state.connectionFilter,
      state.connections,
      state.contributorFilter,
      state.contributors,
      state.dataLocationFilter,
      state.dataLocations,
      state.filterFilter,
      state.filters,
      state.folderFilter,
      state.folders,
      state.ingestFilter,
      state.ingests,
      state.ingestTypeFilter,
      state.ingestTypes,
      state.licenseFilter,
      state.licenses,
      state.ministerFilter,
      state.ministers,
      state.notificationFilter,
      state.notifications,
      state.notificationTemplates,
      state.organizationFilter,
      state.organizations,
      state.mediaTypeFilter,
      state.mediaTypes,
      state.productFilter,
      state.products,
      state.reportFilter,
      state.reportSubscriberFilter,
      state.reports,
      state.reportTemplates,
      state.seriesFilter,
      state.series,
      state.sourceFilter,
      state.sources,
      state.systemMessages,
      state.tagFilter,
      state.tags,
      state.topicFilter,
      state.topics,
      state.rules,
      state.userFilter,
      state.users,
      state.workOrderFilter,
      state.workOrders,
      state.settings,
    ],
  );

  return [state, controller];
};
