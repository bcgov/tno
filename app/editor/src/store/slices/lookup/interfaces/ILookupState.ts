import {
  IActionModel,
  ICategoryModel,
  IContentTypeModel,
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
  actions: IActionModel[];
  sourceActions: ISourceActionModel[];
  sourceMetrics: ISourceMetricModel[];
  categories: ICategoryModel[];
  contentTypes: IContentTypeModel[];
  licenses: ILicenseModel[];
  mediaTypes: IMediaTypeModel[];
  series: ISeriesModel[];
  tags: ITagModel[];
  tonePools: ITonePoolModel[];
  users: IUserModel[];
}
