import { IOptionItem } from 'components/form';
import {
  ContentStatusName,
  IContentActionModel,
  IContentCategoryModel,
  IContentLinkModel,
  IContentTagModel,
  IContentTonePoolModel,
  IFileReferenceModel,
  ITimeTrackingModel,
  WorkflowStatusName,
} from 'hooks/api-editor';

export interface IContentForm {
  id: number;
  uid: string;
  sourceUrl: string;
  headline: string;
  status: ContentStatusName;
  workflowStatus: WorkflowStatusName;
  ownerId: number;
  contentTypeId: number;
  mediaTypeId: number;
  licenseId: number;
  dataSourceId?: number;
  source: string;
  otherSource: string;
  seriesId?: number;
  otherSeries: string;
  page: string;
  summary: string;
  transcription: string;
  actions: IContentActionModel[];
  categories: IContentCategoryModel[];
  tags: IContentTagModel[];
  tone: number | '';
  tonePool?: IOptionItem;
  tonePools: IContentTonePoolModel[];
  timeTrackings: ITimeTrackingModel[];
  fileReferences: IFileReferenceModel[];
  file?: File | null;
  links: IContentLinkModel[];
  publishedOn: string;
  version?: number;
  // Print Content
  section: string;
  edition: string;
  storyType: string;
  byline: string;
}
