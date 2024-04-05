import { IContentListFilter } from 'features/content/interfaces';
import { IContentSearchResult } from 'store/slices';
import { ContentStatusName } from 'tno-core';

import { ITotalsInfo } from '../interfaces';

/**
 * Calculates the new totals based on the search results and filter.
 * @param items An array of search results.
 * @param filter The filter currently applied to the search.
 * @param values The current total values.
 * @returns New total values.
 */
export const calcTotals = (
  items: IContentSearchResult[],
  filter: IContentListFilter,
  values: ITotalsInfo,
) => {
  const topStories = items.filter((i) => i.isTopStory).length;
  const commentary = items.filter((i) => i.isCommentary).length;
  const featuredStories = items.filter((i) => i.isFeaturedStory).length;
  const published = items.filter((i) =>
    [ContentStatusName.Publish, ContentStatusName.Published].includes(i.status),
  ).length;
  if (!filter.topStory && !filter.commentary && !filter.featuredStory && !filter.onlyPublished) {
    return {
      ...values,
      topStories,
      commentary,
      featuredStories,
      published,
    };
  }
  if (filter.topStory) return { ...values, topStories };
  if (filter.commentary) return { ...values, commentary };
  if (filter.featuredStory) return { ...values, featuredStories };
  if (filter.onlyPublished) return { ...values, published };
  return values;
};
