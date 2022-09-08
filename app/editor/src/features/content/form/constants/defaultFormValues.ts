import { ContentStatusName, WorkflowStatusName } from 'hooks/api-editor';

import { IContentForm } from '../interfaces';

export const defaultFormValues: IContentForm = {
  id: 0,
  uid: '',
  sourceUrl: '',
  status: ContentStatusName.Publish,
  workflowStatus: WorkflowStatusName.Received,
  contentTypeId: 0,
  mediaTypeId: 0,
  licenseId: 1,
  dataSourceId: undefined,
  ownerId: 0,
  seriesId: undefined,
  otherSeries: '',
  headline: '',
  summary: '',
  source: '',
  otherSource: '',
  page: '',
  transcription: '',
  publishedOn: '',
  actions: [],
  categories: [],
  tags: [],
  labels: [],
  tone: '',
  tonePools: [],
  timeTrackings: [],
  fileReferences: [],
  links: [],
  // Print Content
  section: '',
  edition: '',
  storyType: '',
  byline: '',
};
