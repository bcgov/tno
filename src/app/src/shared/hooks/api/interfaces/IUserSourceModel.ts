import { ISourceModel } from './ISourceModel';

export interface IUserSourceModel {
  userId: number;
  sourceId: number;
  source?: ISourceModel;
}
