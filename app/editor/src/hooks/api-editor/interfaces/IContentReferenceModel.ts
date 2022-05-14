import { WorkflowStatusName } from '../constants';

export interface IContentReferenceModel {
  source: string;
  uid: string;
  topic: string;
  workflowStatus: WorkflowStatusName;
  offset: number;
  partition: number;
  publishedOn?: string;
  sourceUpdatedOn?: string;
}
