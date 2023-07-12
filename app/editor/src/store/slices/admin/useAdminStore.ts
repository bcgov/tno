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
  IMinisterModel,
  INotificationModel,
  IOrganizationModel,
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

import {
  storeAdminActions,
  storeAdminChartTemplates,
  storeAdminConnections,
  storeAdminContributors,
  storeAdminDataLocations,
  storeAdminFilters,
  storeAdminFolders,
  storeAdminIngests,
  storeAdminIngestTypes,
  storeAdminLicenses,
  storeAdminMinisters,
  storeAdminNotifications,
  storeAdminOrganizations,
  storeAdminProducts,
  storeAdminReports,
  storeAdminReportTemplates,
  storeAdminSeries,
  storeAdminSources,
  storeAdminSystemMessages,
  storeAdminTags,
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
  storeSources: (sources: ISourceModel[] | ActionDelegate<ISourceModel[]>) => void;
  storeConnections: (connections: IConnectionModel[] | ActionDelegate<IConnectionModel[]>) => void;
  storeDataLocations: (
    dataLocations: IDataLocationModel[] | ActionDelegate<IDataLocationModel[]>,
  ) => void;
  storeProducts: (products: IProductModel[] | ActionDelegate<IProductModel[]>) => void;
  storeLicenses: (licenses: ILicenseModel[] | ActionDelegate<ILicenseModel[]>) => void;
  storeIngests: (ingests: IIngestModel[] | ActionDelegate<IIngestModel[]>) => void;
  storeIngestTypes: (ingestTypes: IIngestTypeModel[] | ActionDelegate<IIngestTypeModel[]>) => void;
  storeTopics: (topics: ITopicModel[] | ActionDelegate<ITopicModel[]>) => void;
  storeTopicScoreRules: (
    rules: ITopicScoreRuleModel[] | ActionDelegate<ITopicScoreRuleModel[]>,
  ) => void;
  storeUserFilter: (filter: IUserListFilter | ActionDelegate<IUserListFilter>) => void;
  storeUsers: (users: IPaged<IUserModel> | ActionDelegate<IPaged<IUserModel>>) => void;
  storeTags: (tags: ITagModel[] | ActionDelegate<ITagModel[]>) => void;
  storeSystemMessages: (
    systemMessages: ISystemMessageModel[] | ActionDelegate<ISystemMessageModel[]>,
  ) => void;
  storeActions: (actions: IActionModel[] | ActionDelegate<IActionModel[]>) => void;
  storeSeries: (series: ISeriesModel[] | ActionDelegate<ISeriesModel[]>) => void;
  storeContributors: (
    contributors: IContributorModel[] | ActionDelegate<IContributorModel[]>,
  ) => void;
  storeMinisters: (ministers: IMinisterModel[] | ActionDelegate<IMinisterModel[]>) => void;
  storeOrganizations: (
    organizations: IOrganizationModel[] | ActionDelegate<IOrganizationModel[]>,
  ) => void;
  storeWorkOrderFilter: (
    filter: IWorkOrderListFilter | ActionDelegate<IWorkOrderListFilter>,
  ) => void;
  storeWorkOrders: (
    workOrders: IPaged<IWorkOrderModel> | ActionDelegate<IPaged<IWorkOrderModel>>,
  ) => void;
  storeFolders: (folders: IFolderModel[] | ActionDelegate<IFolderModel[]>) => void;
  storeFilters: (filters: IFilterModel[] | ActionDelegate<IFilterModel[]>) => void;
  storeReports: (reports: IReportModel[] | ActionDelegate<IReportModel[]>) => void;
  storeReportTemplates: (
    reportTemplates: IReportTemplateModel[] | ActionDelegate<IReportTemplateModel[]>,
  ) => void;
  storeChartTemplates: (
    chartTemplates: IChartTemplateModel[] | ActionDelegate<IChartTemplateModel[]>,
  ) => void;
  storeNotifications: (
    notifications: INotificationModel[] | ActionDelegate<INotificationModel[]>,
  ) => void;
}

export const useAdminStore = (props?: IAdminProps): [IAdminState, IAdminStore] => {
  const dispatch = useAppDispatch();
  const state = useAppSelector((store) => store.admin);

  const controller = React.useMemo(
    () => ({
      storeSources: (sources: ISourceModel[] | ActionDelegate<ISourceModel[]>) => {
        if (typeof sources === 'function') {
          dispatch(storeAdminSources(sources(state.sources)));
        } else dispatch(storeAdminSources(sources));
      },
      storeConnections: (connections: IConnectionModel[] | ActionDelegate<IConnectionModel[]>) => {
        if (typeof connections === 'function') {
          dispatch(storeAdminConnections(connections(state.connections)));
        } else dispatch(storeAdminConnections(connections));
      },
      storeDataLocations: (
        dataLocations: IDataLocationModel[] | ActionDelegate<IDataLocationModel[]>,
      ) => {
        if (typeof dataLocations === 'function') {
          dispatch(storeAdminDataLocations(dataLocations(state.dataLocations)));
        } else dispatch(storeAdminDataLocations(dataLocations));
      },
      storeProducts: (products: IProductModel[] | ActionDelegate<IProductModel[]>) => {
        if (typeof products === 'function') {
          dispatch(storeAdminProducts(products(state.products)));
        } else dispatch(storeAdminProducts(products));
      },
      storeLicenses: (licenses: ILicenseModel[] | ActionDelegate<ILicenseModel[]>) => {
        if (typeof licenses === 'function') {
          dispatch(storeAdminLicenses(licenses(state.licenses)));
        } else dispatch(storeAdminLicenses(licenses));
      },
      storeIngests: (ingests: IIngestModel[] | ActionDelegate<IIngestModel[]>) => {
        if (typeof ingests === 'function') {
          dispatch(storeAdminIngests(ingests(state.ingests)));
        } else dispatch(storeAdminIngests(ingests));
      },
      storeIngestTypes: (ingestTypes: IIngestTypeModel[] | ActionDelegate<IIngestTypeModel[]>) => {
        if (typeof ingestTypes === 'function') {
          dispatch(storeAdminIngestTypes(ingestTypes(state.ingestTypes)));
        } else dispatch(storeAdminIngestTypes(ingestTypes));
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
      storeTags: (tags: ITagModel[] | ActionDelegate<ITagModel[]>) => {
        if (typeof tags === 'function') {
          dispatch(storeAdminTags(tags(state.tags)));
        } else dispatch(storeAdminTags(tags));
      },
      storeSystemMessages: (
        systemMessages: ISystemMessageModel[] | ActionDelegate<ISystemMessageModel[]>,
      ) => {
        if (typeof systemMessages === 'function') {
          dispatch(storeAdminSystemMessages(systemMessages(state.systemMessages)));
        } else dispatch(storeAdminSystemMessages(systemMessages));
      },
      storeActions: (actions: IActionModel[] | ActionDelegate<IActionModel[]>) => {
        if (typeof actions === 'function') {
          dispatch(storeAdminActions(actions(state.actions)));
        } else dispatch(storeAdminActions(actions));
      },
      storeSeries: (series: ISeriesModel[] | ActionDelegate<ISeriesModel[]>) => {
        if (typeof series === 'function') {
          dispatch(storeAdminSeries(series(state.series)));
        } else dispatch(storeAdminSeries(series));
      },
      storeContributors: (
        contributors: IContributorModel[] | ActionDelegate<IContributorModel[]>,
      ) => {
        if (typeof contributors === 'function') {
          dispatch(storeAdminContributors(contributors(state.contributors)));
        } else dispatch(storeAdminContributors(contributors));
      },
      storeMinisters: (ministers: IMinisterModel[] | ActionDelegate<IMinisterModel[]>) => {
        if (typeof ministers === 'function') {
          dispatch(storeAdminMinisters(ministers(state.ministers)));
        } else dispatch(storeAdminMinisters(ministers));
      },
      storeOrganizations: (
        organizations: IOrganizationModel[] | ActionDelegate<IOrganizationModel[]>,
      ) => {
        if (typeof organizations === 'function') {
          dispatch(storeAdminOrganizations(organizations(state.organizations)));
        } else dispatch(storeAdminOrganizations(organizations));
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
      storeFolders: (folders: IFolderModel[] | ActionDelegate<IFolderModel[]>) => {
        if (typeof folders === 'function') {
          dispatch(storeAdminFolders(folders(state.folders)));
        } else dispatch(storeAdminFolders(folders));
      },
      storeFilters: (filters: IFilterModel[] | ActionDelegate<IFilterModel[]>) => {
        if (typeof filters === 'function') {
          dispatch(storeAdminFilters(filters(state.filters)));
        } else dispatch(storeAdminFilters(filters));
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
      storeChartTemplates: (
        chartTemplates: IChartTemplateModel[] | ActionDelegate<IChartTemplateModel[]>,
      ) => {
        if (typeof chartTemplates === 'function') {
          dispatch(storeAdminChartTemplates(chartTemplates(state.chartTemplates)));
        } else dispatch(storeAdminChartTemplates(chartTemplates));
      },
      storeNotifications: (
        notifications: INotificationModel[] | ActionDelegate<INotificationModel[]>,
      ) => {
        if (typeof notifications === 'function') {
          dispatch(storeAdminNotifications(notifications(state.notifications)));
        } else dispatch(storeAdminNotifications(notifications));
      },
    }),
    [
      dispatch,
      state.sources,
      state.connections,
      state.dataLocations,
      state.products,
      state.licenses,
      state.ingests,
      state.ingestTypes,
      state.userFilter,
      state.users,
      state.topics,
      state.rules,
      state.tags,
      state.systemMessages,
      state.actions,
      state.series,
      state.contributors,
      state.ministers,
      state.organizations,
      state.workOrderFilter,
      state.workOrders,
      state.folders,
      state.filters,
      state.reports,
      state.reportTemplates,
      state.chartTemplates,
      state.notifications,
    ],
  );

  return [state, controller];
};
