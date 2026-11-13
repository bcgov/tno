import React from 'react';
import { useStore } from 'react-redux';

import { type IUserListFilter } from '@/editor/features/admin/users/interfaces/IUserListFilter';
import { type IWorkOrderListFilter } from '@/editor/features/admin/work-orders/interfaces/IWorkOrderListFilter';
import {
  useAppDispatch,
  useAppSelector,
  type ActionDelegate,
  type RootState,
} from '@/editor/store';
import {
  type IActionModel,
  type IChartTemplateModel,
  type IConnectionModel,
  type IContributorModel,
  type IDataLocationModel,
  type IFilterModel,
  type IFolderModel,
  type IIngestModel,
  type IIngestTypeModel,
  type ILicenseModel,
  type ILLMModel,
  type IMediaTypeModel,
  type IMinisterModel,
  type INotificationModel,
  type INotificationTemplateModel,
  type IOrganizationModel,
  type IPaged,
  type IProductModel,
  type IReportModel,
  type IReportTemplateModel,
  type ISeriesModel,
  type ISettingModel,
  type ISourceModel,
  type ISystemMessageModel,
  type ITagModel,
  type ITopicModel,
  type IUserFilter,
  type IUserModel,
  type IWorkOrderModel,
} from '@/shared';

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
  storeAdminLLMFilter,
  storeAdminLLMs,
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
  storeAdminUserFilter,
  storeAdminUsers,
  storeAdminWorkOrderFilter,
  storeAdminWorkOrders,
} from '.';
import { type IAdminState } from './interfaces';

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
  storeLLMFilter: (filter: string | ActionDelegate<string>) => void;
  storeLLMs: (llms: ILLMModel[] | ActionDelegate<ILLMModel[]>) => void;
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
  // Updates given as a function apply to the store as it is when they are made. The 'state' this
  // render selected can be out of date by then: two requests started together would each apply their
  // update to the same old value, and the last to finish would discard the other's.
  const store = useStore<RootState>();

  const controller = React.useMemo(
    () => ({
      storeActionFilter: (filter: string | ActionDelegate<string>) => {
        if (typeof filter === 'function') {
          dispatch(storeAdminActionFilter(filter(store.getState().admin.actionFilter)));
        } else dispatch(storeAdminActionFilter(filter));
      },
      storeActions: (actions: IActionModel[] | ActionDelegate<IActionModel[]>) => {
        if (typeof actions === 'function') {
          dispatch(storeAdminActions(actions(store.getState().admin.actions)));
        } else dispatch(storeAdminActions(actions));
      },
      storeChartTemplates: (
        chartTemplates: IChartTemplateModel[] | ActionDelegate<IChartTemplateModel[]>,
      ) => {
        if (typeof chartTemplates === 'function') {
          dispatch(storeAdminChartTemplates(chartTemplates(store.getState().admin.chartTemplates)));
        } else dispatch(storeAdminChartTemplates(chartTemplates));
      },
      storeConnectionFilter: (filter: string | ActionDelegate<string>) => {
        if (typeof filter === 'function') {
          dispatch(storeAdminConnectionFilter(filter(store.getState().admin.connectionFilter)));
        } else dispatch(storeAdminConnectionFilter(filter));
      },
      storeConnections: (connections: IConnectionModel[] | ActionDelegate<IConnectionModel[]>) => {
        if (typeof connections === 'function') {
          dispatch(storeAdminConnections(connections(store.getState().admin.connections)));
        } else dispatch(storeAdminConnections(connections));
      },
      storeContributorFilter: (filter: string | ActionDelegate<string>) => {
        if (typeof filter === 'function') {
          dispatch(storeAdminContributorFilter(filter(store.getState().admin.contributorFilter)));
        } else dispatch(storeAdminContributorFilter(filter));
      },
      storeContributors: (
        contributors: IContributorModel[] | ActionDelegate<IContributorModel[]>,
      ) => {
        if (typeof contributors === 'function') {
          dispatch(storeAdminContributors(contributors(store.getState().admin.contributors)));
        } else dispatch(storeAdminContributors(contributors));
      },
      storeDataLocationFilter: (filter: string | ActionDelegate<string>) => {
        if (typeof filter === 'function') {
          dispatch(storeAdminDataLocationFilter(filter(store.getState().admin.dataLocationFilter)));
        } else dispatch(storeAdminDataLocationFilter(filter));
      },
      storeDataLocations: (
        dataLocations: IDataLocationModel[] | ActionDelegate<IDataLocationModel[]>,
      ) => {
        if (typeof dataLocations === 'function') {
          dispatch(storeAdminDataLocations(dataLocations(store.getState().admin.dataLocations)));
        } else dispatch(storeAdminDataLocations(dataLocations));
      },
      storeFilterFilter: (filter: string | ActionDelegate<string>) => {
        if (typeof filter === 'function') {
          dispatch(storeAdminFilterFilter(filter(store.getState().admin.filterFilter)));
        } else dispatch(storeAdminFilterFilter(filter));
      },
      storeFilters: (filters: IFilterModel[] | ActionDelegate<IFilterModel[]>) => {
        if (typeof filters === 'function') {
          dispatch(storeAdminFilters(filters(store.getState().admin.filters)));
        } else dispatch(storeAdminFilters(filters));
      },
      storeFolderFilter: (filter: string | ActionDelegate<string>) => {
        if (typeof filter === 'function') {
          dispatch(storeAdminFolderFilter(filter(store.getState().admin.folderFilter)));
        } else dispatch(storeAdminFolderFilter(filter));
      },
      storeFolders: (folders: IFolderModel[] | ActionDelegate<IFolderModel[]>) => {
        if (typeof folders === 'function') {
          dispatch(storeAdminFolders(folders(store.getState().admin.folders)));
        } else dispatch(storeAdminFolders(folders));
      },
      storeIngestFilter: (filter: string | ActionDelegate<string>) => {
        if (typeof filter === 'function') {
          dispatch(storeAdminIngestFilter(filter(store.getState().admin.ingestFilter)));
        } else dispatch(storeAdminIngestFilter(filter));
      },
      storeIngests: (ingests: IIngestModel[] | ActionDelegate<IIngestModel[]>) => {
        if (typeof ingests === 'function') {
          dispatch(storeAdminIngests(ingests(store.getState().admin.ingests)));
        } else dispatch(storeAdminIngests(ingests));
      },
      storeIngestTypeFilter: (filter: string | ActionDelegate<string>) => {
        if (typeof filter === 'function') {
          dispatch(storeAdminIngestTypeFilter(filter(store.getState().admin.ingestTypeFilter)));
        } else dispatch(storeAdminIngestTypeFilter(filter));
      },
      storeIngestTypes: (ingestTypes: IIngestTypeModel[] | ActionDelegate<IIngestTypeModel[]>) => {
        if (typeof ingestTypes === 'function') {
          dispatch(storeAdminIngestTypes(ingestTypes(store.getState().admin.ingestTypes)));
        } else dispatch(storeAdminIngestTypes(ingestTypes));
      },
      storeLicenseFilter: (filter: string | ActionDelegate<string>) => {
        if (typeof filter === 'function') {
          dispatch(storeAdminLicenseFilter(filter(store.getState().admin.licenseFilter)));
        } else dispatch(storeAdminLicenseFilter(filter));
      },
      storeLicenses: (licenses: ILicenseModel[] | ActionDelegate<ILicenseModel[]>) => {
        if (typeof licenses === 'function') {
          dispatch(storeAdminLicenses(licenses(store.getState().admin.licenses)));
        } else dispatch(storeAdminLicenses(licenses));
      },
      storeLLMFilter: (filter: string | ActionDelegate<string>) => {
        if (typeof filter === 'function') {
          dispatch(storeAdminLLMFilter(filter(store.getState().admin.llmFilter)));
        } else dispatch(storeAdminLLMFilter(filter));
      },
      storeLLMs: (llms: ILLMModel[] | ActionDelegate<ILLMModel[]>) => {
        if (typeof llms === 'function') {
          dispatch(storeAdminLLMs(llms(store.getState().admin.llms)));
        } else dispatch(storeAdminLLMs(llms));
      },
      storeMinisterFilter: (filter: string | ActionDelegate<string>) => {
        if (typeof filter === 'function') {
          dispatch(storeAdminMinisterFilter(filter(store.getState().admin.ministerFilter)));
        } else dispatch(storeAdminMinisterFilter(filter));
      },
      storeMinisters: (ministers: IMinisterModel[] | ActionDelegate<IMinisterModel[]>) => {
        if (typeof ministers === 'function') {
          dispatch(storeAdminMinisters(ministers(store.getState().admin.ministers)));
        } else dispatch(storeAdminMinisters(ministers));
      },
      storeNotificationFilter: (filter: string | ActionDelegate<string>) => {
        if (typeof filter === 'function') {
          dispatch(storeAdminNotificationFilter(filter(store.getState().admin.notificationFilter)));
        } else dispatch(storeAdminNotificationFilter(filter));
      },
      storeNotifications: (
        notifications: INotificationModel[] | ActionDelegate<INotificationModel[]>,
      ) => {
        if (typeof notifications === 'function') {
          dispatch(storeAdminNotifications(notifications(store.getState().admin.notifications)));
        } else dispatch(storeAdminNotifications(notifications));
      },
      storeNotificationTemplates: (
        templates: INotificationTemplateModel[] | ActionDelegate<INotificationTemplateModel[]>,
      ) => {
        if (typeof templates === 'function') {
          dispatch(
            storeAdminNotificationTemplates(
              templates(store.getState().admin.notificationTemplates),
            ),
          );
        } else dispatch(storeAdminNotificationTemplates(templates));
      },
      storeOrganizationFilter: (filter: string | ActionDelegate<string>) => {
        if (typeof filter === 'function') {
          dispatch(storeAdminOrganizationFilter(filter(store.getState().admin.organizationFilter)));
        } else dispatch(storeAdminOrganizationFilter(filter));
      },
      storeOrganizations: (
        organizations: IOrganizationModel[] | ActionDelegate<IOrganizationModel[]>,
      ) => {
        if (typeof organizations === 'function') {
          dispatch(storeAdminOrganizations(organizations(store.getState().admin.organizations)));
        } else dispatch(storeAdminOrganizations(organizations));
      },
      storeMediaTypeFilter: (filter: string | ActionDelegate<string>) => {
        if (typeof filter === 'function') {
          dispatch(storeAdminMediaTypeFilter(filter(store.getState().admin.mediaTypeFilter)));
        } else dispatch(storeAdminMediaTypeFilter(filter));
      },
      storeMediaTypes: (mediaTypes: IMediaTypeModel[] | ActionDelegate<IMediaTypeModel[]>) => {
        if (typeof mediaTypes === 'function') {
          dispatch(storeAdminMediaTypes(mediaTypes(store.getState().admin.mediaTypes)));
        } else dispatch(storeAdminMediaTypes(mediaTypes));
      },
      storeProductFilter: (filter: string | ActionDelegate<string>) => {
        if (typeof filter === 'function') {
          dispatch(storeAdminProductFilter(filter(store.getState().admin.productFilter)));
        } else dispatch(storeAdminProductFilter(filter));
      },
      storeProducts: (products: IProductModel[] | ActionDelegate<IProductModel[]>) => {
        if (typeof products === 'function') {
          dispatch(storeAdminProducts(products(store.getState().admin.products)));
        } else dispatch(storeAdminProducts(products));
      },
      storeReportFilter: (filter: string | ActionDelegate<string>) => {
        if (typeof filter === 'function') {
          dispatch(storeAdminReportFilter(filter(store.getState().admin.reportFilter)));
        } else dispatch(storeAdminReportFilter(filter));
      },
      storeReportSubscriberFilter: (filter: IUserFilter | ActionDelegate<IUserFilter>) => {
        if (typeof filter === 'function') {
          dispatch(
            storeAdminReportSubscriberFilter(filter(store.getState().admin.reportSubscriberFilter)),
          );
        } else dispatch(storeAdminReportSubscriberFilter(filter));
      },
      storeReports: (reports: IReportModel[] | ActionDelegate<IReportModel[]>) => {
        if (typeof reports === 'function') {
          dispatch(storeAdminReports(reports(store.getState().admin.reports)));
        } else dispatch(storeAdminReports(reports));
      },
      storeReportTemplates: (
        reportTemplates: IReportTemplateModel[] | ActionDelegate<IReportTemplateModel[]>,
      ) => {
        if (typeof reportTemplates === 'function') {
          dispatch(
            storeAdminReportTemplates(reportTemplates(store.getState().admin.reportTemplates)),
          );
        } else dispatch(storeAdminReportTemplates(reportTemplates));
      },
      storeSeriesFilter: (filter: string | ActionDelegate<string>) => {
        if (typeof filter === 'function') {
          dispatch(storeAdminSeriesFilter(filter(store.getState().admin.seriesFilter)));
        } else dispatch(storeAdminSeriesFilter(filter));
      },
      storeSeries: (series: ISeriesModel[] | ActionDelegate<ISeriesModel[]>) => {
        if (typeof series === 'function') {
          dispatch(storeAdminSeries(series(store.getState().admin.series)));
        } else dispatch(storeAdminSeries(series));
      },
      storeSourceFilter: (filter: string | ActionDelegate<string>) => {
        if (typeof filter === 'function') {
          dispatch(storeAdminSourceFilter(filter(store.getState().admin.sourceFilter)));
        } else dispatch(storeAdminSourceFilter(filter));
      },
      storeSources: (sources: ISourceModel[] | ActionDelegate<ISourceModel[]>) => {
        if (typeof sources === 'function') {
          dispatch(storeAdminSources(sources(store.getState().admin.sources)));
        } else dispatch(storeAdminSources(sources));
      },
      storeSystemMessages: (
        systemMessages: ISystemMessageModel[] | ActionDelegate<ISystemMessageModel[]>,
      ) => {
        if (typeof systemMessages === 'function') {
          dispatch(storeAdminSystemMessages(systemMessages(store.getState().admin.systemMessages)));
        } else dispatch(storeAdminSystemMessages(systemMessages));
      },
      storeTagFilter: (filter: string | ActionDelegate<string>) => {
        if (typeof filter === 'function') {
          dispatch(storeAdminTagFilter(filter(store.getState().admin.tagFilter)));
        } else dispatch(storeAdminTagFilter(filter));
      },
      storeTags: (tags: ITagModel[] | ActionDelegate<ITagModel[]>) => {
        if (typeof tags === 'function') {
          dispatch(storeAdminTags(tags(store.getState().admin.tags)));
        } else dispatch(storeAdminTags(tags));
      },
      storeTopicFilter: (filter: string | ActionDelegate<string>) => {
        if (typeof filter === 'function') {
          dispatch(storeAdminTopicFilter(filter(store.getState().admin.topicFilter)));
        } else dispatch(storeAdminTopicFilter(filter));
      },
      storeTopics: (topics: ITopicModel[] | ActionDelegate<ITopicModel[]>) => {
        if (typeof topics === 'function') {
          dispatch(storeAdminTopics(topics(store.getState().admin.topics)));
        } else dispatch(storeAdminTopics(topics));
      },
      storeUserFilter: (filter: IUserListFilter | ActionDelegate<IUserListFilter>) => {
        if (typeof filter === 'function') {
          dispatch(storeAdminUserFilter(filter(store.getState().admin.userFilter)));
        } else dispatch(storeAdminUserFilter(filter));
      },
      storeUsers: (users: IPaged<IUserModel> | ActionDelegate<IPaged<IUserModel>>) => {
        if (typeof users === 'function') {
          dispatch(storeAdminUsers(users(store.getState().admin.users)));
        } else dispatch(storeAdminUsers(users));
      },
      storeWorkOrderFilter: (
        filter: IWorkOrderListFilter | ActionDelegate<IWorkOrderListFilter>,
      ) => {
        if (typeof filter === 'function') {
          dispatch(storeAdminWorkOrderFilter(filter(store.getState().admin.workOrderFilter)));
        } else dispatch(storeAdminWorkOrderFilter(filter));
      },
      storeWorkOrders: (
        workOrders: IPaged<IWorkOrderModel> | ActionDelegate<IPaged<IWorkOrderModel>>,
      ) => {
        if (typeof workOrders === 'function') {
          dispatch(storeAdminWorkOrders(workOrders(store.getState().admin.workOrders)));
        } else dispatch(storeAdminWorkOrders(workOrders));
      },
      storeSettings: (settings: ISettingModel[] | ActionDelegate<ISettingModel[]>) => {
        if (typeof settings === 'function') {
          dispatch(storeAdminSettings(settings(store.getState().admin.settings)));
        } else dispatch(storeAdminSettings(settings));
      },
    }),
    [dispatch, store],
  );

  return [state, controller];
};
