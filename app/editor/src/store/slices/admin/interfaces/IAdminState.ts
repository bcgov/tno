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

export interface IAdminState {
  actions: IActionModel[];
  chartTemplates: IChartTemplateModel[];
  connections: IConnectionModel[];
  contributors: IContributorModel[];
  dataLocations: IDataLocationModel[];
  folders: IFolderModel[];
  filters: IFilterModel[];
  ingests: IIngestModel[];
  ingestTypes: IIngestTypeModel[];
  licenses: ILicenseModel[];
  ministers: IMinisterModel[];
  notifications: INotificationModel[];
  organizations: IOrganizationModel[];
  products: IProductModel[];
  reports: IReportModel[];
  reportTemplates: IReportTemplateModel[];
  rules: ITopicScoreRuleModel[];
  series: ISeriesModel[];
  sources: ISourceModel[];
  systemMessages: ISystemMessageModel[];
  tags: ITagModel[];
  topics: ITopicModel[];
  userFilter: IUserListFilter;
  users: IPaged<IUserModel>;
  workOrderFilter: IWorkOrderListFilter;
  workOrders: IPaged<IWorkOrderModel>;
  settings: ISettingModel[];
}
