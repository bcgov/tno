import {
  IActionModel,
  ICategoryModel,
  IClaimModel,
  ILicenseModel,
  IMediaTypeModel,
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
  claims: IClaimModel[];
  products: IProductModel[];
  sources: ISourceModel[];
  licenses: ILicenseModel[];
  mediaTypes: IMediaTypeModel[];
  roles: IRoleModel[];
  series: ISeriesModel[];
  sourceActions: ISourceActionModel[];
  metrics: IMetricModel[];
  tags: ITagModel[];
  tonePools: ITonePoolModel[];
  users: IUserModel[];
}
