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

export interface IAdminState {
  actionFilter: string;
  actions: IActionModel[];
  chartTemplates: IChartTemplateModel[];
  connectionFilter: string;
  connections: IConnectionModel[];
  contributorFilter: string;
  contributors: IContributorModel[];
  dataLocationFilter: string;
  dataLocations: IDataLocationModel[];
  folderFilter: string;
  folders: IFolderModel[];
  filterFilter: string;
  filters: IFilterModel[];
  ingestFilter: string;
  ingests: IIngestModel[];
  ingestTypeFilter: string;
  ingestTypes: IIngestTypeModel[];
  licenseFilter: string;
  licenses: ILicenseModel[];
  mediaTypeFilter: string;
  mediaTypes: IMediaTypeModel[];
  ministerFilter: string;
  ministers: IMinisterModel[];
  notificationFilter: string;
  notifications: INotificationModel[];
  notificationTemplates: INotificationTemplateModel[];
  organizationFilter: string;
  organizations: IOrganizationModel[];
  productFilter: string;
  products: IProductModel[];
  reportFilter: string;
  reportSubscriberFilter: IUserFilter;
  reports: IReportModel[];
  reportTemplates: IReportTemplateModel[];
  rules: ITopicScoreRuleModel[];
  seriesFilter: string;
  series: ISeriesModel[];
  sourceFilter: string;
  sources: ISourceModel[];
  systemMessages: ISystemMessageModel[];
  tagFilter: string;
  tags: ITagModel[];
  topicFilter: string;
  topics: ITopicModel[];
  userFilter: IUserListFilter;
  users: IPaged<IUserModel>;
  workOrderFilter: IWorkOrderListFilter;
  workOrders: IPaged<IWorkOrderModel>;
  settings: ISettingModel[];
}
