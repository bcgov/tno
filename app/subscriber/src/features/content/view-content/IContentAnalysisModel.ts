/** A content item's structured analysis. */
export interface IContentAnalysisModel {
  contentId: number;
  isMetadataOnly: boolean;
  model: string;
  summary: string;
  keyFacts: { statement: string; isInferred?: boolean }[];
  entities: {
    type: string;
    name: string;
    aliases?: string[];
    roles?: string[];
    isAmbiguous?: boolean;
  }[];
  places: { name: string; role: string }[];
  topics: { label: string; relevance: number }[];
  staffTopic?: string;
  events: { actor: string; action: string; date?: string; location?: string }[];
  analyzedOn: string;
}
