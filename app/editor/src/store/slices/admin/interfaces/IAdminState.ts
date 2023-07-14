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
  ISourceModel,
  ISystemMessageModel,
  ITagModel,
  ITopicModel,
  ITopicScoreRuleModel,
  IUserModel,
  IWorkOrderModel,
} from 'tno-core';

export interface IAdminState {
  sources: ISourceModel[];
  connections: IConnectionModel[];
  dataLocations: IDataLocationModel[];
  products: IProductModel[];
  ingests: IIngestModel[];
  ingestTypes: IIngestTypeModel[];
  userFilter: IUserListFilter;
  users: IPaged<IUserModel>;
  topics: ITopicModel[];
  rules: ITopicScoreRuleModel[];
  tags: ITagModel[];
  systemMessages: ISystemMessageModel[];
  actions: IActionModel[];
  series: ISeriesModel[];
  contributors: IContributorModel[];
  licenses: ILicenseModel[];
  ministers: IMinisterModel[];
  organizations: IOrganizationModel[];
  folders: IFolderModel[];
  filters: IFilterModel[];
  workOrderFilter: IWorkOrderListFilter;
  workOrders: IPaged<IWorkOrderModel>;
  reports: IReportModel[];
  reportTemplates: IReportTemplateModel[];
  chartTemplates: IChartTemplateModel[];
  notifications: INotificationModel[];
}
