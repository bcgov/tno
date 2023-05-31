import { IMinisterModel } from 'store/hooks/subscriber/interfaces/IMinisterModel';
import {
  IActionModel,
  ICacheModel,
  IContributorModel,
  IHolidayModel,
  ILicenseModel,
  IProductModel,
  ISeriesModel,
  ISourceActionModel,
  ISourceModel,
  ITagModel,
  ITonePoolModel,
  ITopicModel,
} from 'tno-core';

export interface ILookupState {
  cache: ICacheModel[];
  actions: IActionModel[];
  topics: ITopicModel[];
  ministers: IMinisterModel[];
  products: IProductModel[];
  licenses: ILicenseModel[];
  series: ISeriesModel[];
  contributors: IContributorModel[];
  sources: ISourceModel[];
  sourceActions: ISourceActionModel[];
  tags: ITagModel[];
  tonePools: ITonePoolModel[];
  holidays: IHolidayModel[];
}
