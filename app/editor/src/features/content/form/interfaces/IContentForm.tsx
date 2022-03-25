import {
  ContentStatus,
  IActionValueModel,
  ICategoryModel,
  ITagModel,
  ITimeTrackingModel,
  WorkflowStatus,
} from 'hooks';

export interface IContentForm {
  timeTrackings: ITimeTrackingModel[];
  actions: IActionValueModel[];
  categories: ICategoryModel[];
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
