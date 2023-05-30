import {
  IActionModel,
  ILicenseModel,
  IProductModel,
  ISeriesModel,
  ISourceModel,
  ITagModel,
  ITonePoolModel,
  ITopicModel,
} from 'tno-core';

import { IMinisterModel } from '../subscriber/interfaces/IMinisterModel';

export interface ILookupModel {
  actions: IActionModel[];
  topics: ITopicModel[];
  products: IProductModel[];
  sources: ISourceModel[];
  licenses: ILicenseModel[];
  series: ISeriesModel[];
  tags: ITagModel[];
  tonePools: ITonePoolModel[];
  ministers: IMinisterModel[];
}
