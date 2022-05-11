import {
  IActionModel,
  ICategoryModel,
  IClaimModel,
  IContentTypeModel,
  IDataLocationModel,
  ILicenseModel,
  IMediaTypeModel,
  IRoleModel,
  ISeriesModel,
  ISourceActionModel,
  ISourceMetricModel,
  ITagModel,
  ITonePoolModel,
  IUserModel,
} from '.';
import { IDataSourceModel } from './IDataSourceModel';

export interface ILookupModel {
  actions: IActionModel[];
  categories: ICategoryModel[];
  claims: IClaimModel[];
  contentTypes: IContentTypeModel[];
  dataSources: IDataSourceModel[];
  dataLocations: IDataLocationModel[];
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
