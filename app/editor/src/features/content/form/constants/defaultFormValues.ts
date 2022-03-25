import { ContentStatus, WorkflowStatus } from 'hooks/api-editor';

import { IContentForm } from '../interfaces';

export const defaultFormValues: IContentForm = {
  id: 0,
  uid: '',
  sourceUrl: '',
  status: ContentStatus.Publish,
  workflowStatus: WorkflowStatus.Received,
  contentTypeId: 0,
  mediaTypeId: 0,
  licenseId: 0,
  ownerId: 0,
  seriesId: 0,
  headline: '',
  summary: '',
  source: '',
  page: '',
  transcription: '',
  publishedOn: '',
  timeTrackings: [],
  actions: [],
  tags: [],
  categories: [],
  // Print Content
  section: '',
  edition: '',
  storyType: '',
  byline: '',
};
