import {
  IActionModel,
  ICategoryModel,
  IContentTypeModel,
  ILicenseModel,
  IMediaTypeModel,
  ISeriesModel,
  ITagModel,
  ITonePoolModel,
  IUserModel,
} from 'hooks';

export interface ILookupState {
  actions: IActionModel[];
  categories: ICategoryModel[];
  contentTypes: IContentTypeModel[];
  licenses: ILicenseModel[];
  mediaTypes: IMediaTypeModel[];
  series: ISeriesModel[];
  tags: ITagModel[];
  tonePools: ITonePoolModel[];
  users: IUserModel[];
}
