import { IContentModel } from '@/shared';

export interface IContentSearchResult extends IContentModel {
  hasTranscript: boolean;
  mediaUrl?: string | undefined;
  displayMedia?: boolean;
  ministerName?: string;
  ministerMentions?: string[];
}
