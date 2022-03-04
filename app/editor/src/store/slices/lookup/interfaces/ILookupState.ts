import {
  IActionModel,
  ICategoryModel,
  IContentTypeModel,
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
  mediaTypes: IMediaTypeModel[];
  series: ISeriesModel[];
  tags: ITagModel[];
  tonePools: ITonePoolModel[];
  users: IUserModel[];
}
