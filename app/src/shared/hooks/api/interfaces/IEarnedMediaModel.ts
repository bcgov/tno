import { IAuditColumnsModel, ISourceModel } from '.';
import { ContentTypeName } from '../constants';

export interface IEarnedMediaModel extends IAuditColumnsModel {
  sourceId: number;
  source?: ISourceModel;
  contentType: ContentTypeName;
  lengthOfContent: number;
  rate: number;
}
