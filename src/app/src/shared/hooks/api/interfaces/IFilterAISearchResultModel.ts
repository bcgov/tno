import { IContentModel } from '.';

/** The part of an Elasticsearch search response the filter prompt workbench lists. */
export interface IFilterAISearchResultModel {
  hits: {
    /** The number of matching stories; an object when Elasticsearch reports a lower bound. */
    total?: number | { value: number; relation?: string };
    hits: { _id?: string; _source?: IContentModel }[];
  };
}
