import {
  IActionModel,
  IDataLocationModel,
  IIngestTypeModel,
  ILicenseModel,
  IMetricModel,
  IProductModel,
  IRoleModel,
  ISeriesModel,
  ISourceActionModel,
  ISourceModel,
  ITagModel,
  ITonePoolModel,
  ITopicModel,
  ITopicScoreRuleModel,
  IUserModel,
} from '.';

export interface ILookupModel {
  actions: IActionModel[];
  topics: ITopicModel[];
  rules: ITopicScoreRuleModel[];
  products: IProductModel[];
  sources: ISourceModel[];
  licenses: ILicenseModel[];
  ingestTypes: IIngestTypeModel[];
  roles: IRoleModel[];
  series: ISeriesModel[];
  sourceActions: ISourceActionModel[];
  metrics: IMetricModel[];
  tags: ITagModel[];
  tonePools: ITonePoolModel[];
  users: IUserModel[];
  dataLocations: IDataLocationModel[];
}
