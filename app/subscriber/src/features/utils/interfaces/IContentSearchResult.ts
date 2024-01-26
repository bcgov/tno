import { IContentModel } from 'tno-core';

export interface IContentSearchResult extends IContentModel {
  hasTranscript: boolean;
  mediaUrl?: string | undefined;
  displayMedia?: boolean;
}
