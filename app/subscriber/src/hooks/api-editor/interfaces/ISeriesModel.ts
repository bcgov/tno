import { ISortableModel } from '.';

export interface ISeriesModel extends ISortableModel<number> {
  autoTranscribe: boolean;
  useInTopics: boolean;
}
