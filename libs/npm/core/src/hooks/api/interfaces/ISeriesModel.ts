import { ISortableModel, ISourceModel } from '.';

export interface ISeriesModel extends ISortableModel<number> {
  sourceId?: number;
  source?: ISourceModel;
  autoTranscribe: boolean;
  isOther: boolean;
  useInTopics: boolean;
}
