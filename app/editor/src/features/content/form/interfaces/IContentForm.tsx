import { ContentStatus, WorkflowStatus, ITimeTrackingModel, IActionModel, ITagModel } from 'hooks/api-editor';

export interface IContentForm {
  timeTrackings: ITimeTrackingModel[];
  actions: IActionModel[];
  tags: ITagModel[];
  id: number;
  uid: string;
  sourceUrl: string;
  status: ContentStatus;
  workflowStatus: WorkflowStatus;
  ownerId: number;
  contentTypeId: number;
  mediaTypeId: number;
  licenseId: number;
  seriesId?: number;
  headline: string;
  source: string;
  page: string;
  summary: string;
  transcription: string;
  publishedOn: string;
  // Print Content
  section: string;
  edition: string;
  storyType: string;
  byline: string;
}
