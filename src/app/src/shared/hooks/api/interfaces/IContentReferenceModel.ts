import { WorkflowStatusName } from '../constants';

export interface IContentReferenceModel {
  source: string;
  uid: string;
  topic: string;
  status: WorkflowStatusName;
  publishedOn?: string;
  sourceUpdatedOn?: string;
}
