import {
  IActionModel,
  ICategoryModel,
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
  IUserModel,
} from '.';

export interface ILookupModel {
  actions: IActionModel[];
  categories: ICategoryModel[];
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
}
