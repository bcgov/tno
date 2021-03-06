import {
  IActionModel,
  ICacheModel,
  ICategoryModel,
  IClaimModel,
  IContentTypeModel,
  IDataLocationModel,
  IDataSourceModel,
  ILicenseModel,
  IMediaTypeModel,
  IRoleModel,
  ISeriesModel,
  ISourceActionModel,
  ISourceMetricModel,
  ITagModel,
  ITonePoolModel,
  IUserModel,
} from 'hooks/api-editor';

export interface ILookupState {
  cache: ICacheModel[];
  actions: IActionModel[];
  categories: ICategoryModel[];
  claims: IClaimModel[];
  contentTypes: IContentTypeModel[];
  dataLocations: IDataLocationModel[];
  dataSources: IDataSourceModel[];
  licenses: ILicenseModel[];
  mediaTypes: IMediaTypeModel[];
  roles: IRoleModel[];
  series: ISeriesModel[];
  sourceActions: ISourceActionModel[];
  sourceMetrics: ISourceMetricModel[];
  tags: ITagModel[];
  tonePools: ITonePoolModel[];
  users: IUserModel[];
}
