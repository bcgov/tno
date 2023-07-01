import { ContentTypeName } from '../constants';
import { IAuditColumnsModel, ISourceModel } from '.';

export interface IEarnedMediaModel extends IAuditColumnsModel {
  sourceId: number;
  source?: ISourceModel;
  contentType: ContentTypeName;
  lengthOfContent: number;
  rate: number;
}
