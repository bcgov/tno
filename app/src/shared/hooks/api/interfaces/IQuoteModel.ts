import { IAuditColumnsModel } from './IAuditColumnsModel';

export interface IQuoteModel extends IAuditColumnsModel {
  id: number;
  contentId: number;
  statement: string;
  byline: string;
  isRelevant: boolean;
}
