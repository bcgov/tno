import {
  IActionModel,
  ICacheModel,
  ICategoryModel,
  IContentTypeModel,
  IDataLocationModel,
  IDataSourceModel,
  ILicenseModel,
  IMediaTypeModel,
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
  dataLocations: IDataLocationModel[];
  sourceActions: ISourceActionModel[];
  sourceMetrics: ISourceMetricModel[];
  categories: ICategoryModel[];
  contentTypes: IContentTypeModel[];
  licenses: ILicenseModel[];
  mediaTypes: IMediaTypeModel[];
  dataSources: IDataSourceModel[];
  series: ISeriesModel[];
  tags: ITagModel[];
  tonePools: ITonePoolModel[];
  users: IUserModel[];
}
