import { type IMediaTypeModel, type ISeriesModel } from 'tno-core';

export interface ISeriesForm extends Omit<ISeriesModel, 'sourceId'> {
  sourceId?: number | '';
  mediaTypeSearchMappings?: IMediaTypeModel[];
}
