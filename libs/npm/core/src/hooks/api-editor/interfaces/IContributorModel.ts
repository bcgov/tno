import { ISortableModel, ISourceModel } from '.';

export interface IContributorModel extends ISortableModel<number> {
  aliases?: string;
  autoTranscribe: boolean;
  isPress: boolean;
  source?: ISourceModel;
  sourceId?: number;
}
