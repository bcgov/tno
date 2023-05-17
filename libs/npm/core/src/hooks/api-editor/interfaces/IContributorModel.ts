import { ISortableModel, ISourceModel } from '.';

export interface IContributorModel extends ISortableModel<number> {
  sourceId?: number;
  source?: ISourceModel;
  autoTranscribe: boolean;
}
