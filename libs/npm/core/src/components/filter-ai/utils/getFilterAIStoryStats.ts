import { IFilterAISearchResultModel } from '../../../hooks';

export interface IFilterAIStoryStats {
  /** How many stories match the query. */
  total: number;
  /** Whether Elasticsearch stopped counting, so the total is a lower bound. */
  totalIsLowerBound: boolean;
  /** How many distinct sources the listed stories come from. */
  sources: number;
  /** The media types of the listed stories with their counts, most common first (at most five). */
  mediaTypes: [string, number][];
  /** How many listed stories have a positive, neutral or negative tone, or none. */
  sentiment: { positive: number; neutral: number; negative: number; none: number };
  /** The earliest published date among the listed stories. */
  earliest?: string;
  /** The latest published date among the listed stories. */
  latest?: string;
}

/**
 * Summarizes the stories a query found, for the workbench's list header.
 * @param response The search response, if any.
 * @returns The stats.
 */
export const getFilterAIStoryStats = (
  response?: IFilterAISearchResultModel,
): IFilterAIStoryStats => {
  const stats: IFilterAIStoryStats = {
    total: 0,
    totalIsLowerBound: false,
    sources: 0,
    mediaTypes: [],
    sentiment: { positive: 0, neutral: 0, negative: 0, none: 0 },
  };
  if (!response) return stats;

  const total = response.hits.total;
  if (typeof total === 'number') stats.total = total;
  else if (total) {
    stats.total = total.value;
    stats.totalIsLowerBound = total.relation === 'gte';
  }

  const sources = new Set<string>();
  const mediaTypes = new Map<string, number>();
  for (const hit of response.hits.hits) {
    const content = hit._source;
    if (!content) continue;
    const source = content.source?.code ?? content.otherSource;
    if (source) sources.add(source);
    const mediaType = content.mediaType?.name;
    if (mediaType) mediaTypes.set(mediaType, (mediaTypes.get(mediaType) ?? 0) + 1);
    const tone = content.tonePools?.[0]?.value;
    if (tone === undefined || tone === null) stats.sentiment.none++;
    else if (tone > 0) stats.sentiment.positive++;
    else if (tone < 0) stats.sentiment.negative++;
    else stats.sentiment.neutral++;
    if (content.publishedOn) {
      if (!stats.earliest || content.publishedOn < stats.earliest)
        stats.earliest = content.publishedOn;
      if (!stats.latest || content.publishedOn > stats.latest) stats.latest = content.publishedOn;
    }
  }
  stats.sources = sources.size;
  stats.mediaTypes = Array.from(mediaTypes.entries())
    .sort((a, b) => b[1] - a[1])
    .slice(0, 5);
  return stats;
};
