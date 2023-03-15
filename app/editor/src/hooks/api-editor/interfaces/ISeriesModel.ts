import { ISortableModel, ISourceModel } from '.';

export interface ISeriesModel extends ISortableModel<number> {
  sourceId?: number;
  source?: ISourceModel;
  autoTranscribe: boolean;
  useInTopics: boolean;
}
