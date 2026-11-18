import { IMediaTypeModel, ISortableModel, ISourceModel } from '.';

export interface ISeriesModel extends ISortableModel<number> {
  sourceId?: number;
  source?: ISourceModel;
  mediaTypeId?: number;
  mediaTypeSearchMappings?: IMediaTypeModel[];
  autoTranscribe: boolean;
  isOther: boolean;
  useInTopics: boolean;
  isCBRASource: boolean;
}
