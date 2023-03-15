import { ISeriesModel } from 'hooks';

export interface ISeriesForm extends Omit<ISeriesModel, 'sourceId'> {
  sourceId?: number | '';
}
