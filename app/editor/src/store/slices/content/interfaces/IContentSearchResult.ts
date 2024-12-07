import { ContentStatusName, ContentTypeName, IContentModel, WorkOrderStatusName } from 'tno-core';

export interface IContentSearchResult {
  id: number;
  headline: string;
  status: ContentStatusName;
  contentType: ContentTypeName;
  byline: string;
  edition: string;
  section: string;
  page: string;
  publishedOn: string;
  source?: string;
  otherSource: string;
  mediaType?: string;
  series?: string;
  ownerId?: number;
  owner?: string;
  isHidden: boolean;
  isApproved: boolean;
  hasTranscript: boolean;
  isTopStory: boolean;
  isCommentary: boolean;
  isFeaturedStory: boolean;
  isSelected?: boolean;
  transcriptStatus?: WorkOrderStatusName;
  original: IContentModel;
  version?: number;
  isCBRAUnqualified: boolean;
}
