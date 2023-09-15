import { ContentStatusName, ContentTypeName } from 'tno-core';

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
  product?: string;
  series?: string;
  owner?: string;
  isHidden: boolean;
  isApproved: boolean;
  hasTranscript: boolean;
  version?: number;

  // React-Table Properties
  // TODO: Should not be part of the API interface.
  isSelected?: boolean;
}
