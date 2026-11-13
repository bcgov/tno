import { type IMediaTypeModel, type ISeriesModel } from '@/shared';

export interface ISeriesForm extends Omit<ISeriesModel, 'sourceId'> {
  sourceId?: number | '';
  mediaTypeSearchMappings?: IMediaTypeModel[];
}
