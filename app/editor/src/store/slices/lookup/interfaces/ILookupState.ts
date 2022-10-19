import {
  IActionModel,
  ICacheModel,
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
} from 'hooks/api-editor';

export interface ILookupState {
  cache: ICacheModel[];
  actions: IActionModel[];
  categories: ICategoryModel[];
  products: IProductModel[];
  licenses: ILicenseModel[];
  ingestTypes: IIngestTypeModel[];
  roles: IRoleModel[];
  series: ISeriesModel[];
  sources: ISourceModel[];
  sourceActions: ISourceActionModel[];
  metrics: IMetricModel[];
  tags: ITagModel[];
  tonePools: ITonePoolModel[];
  users: IUserModel[];
}
