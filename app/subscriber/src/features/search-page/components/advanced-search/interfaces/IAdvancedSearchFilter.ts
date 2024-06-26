import { ContentTypeName } from 'tno-core';

export interface IAdvancedSearchFilter {
  /** array of actions to filter by */
  actions?: string[];
  /** keeps track of which field the search term will query */
  inHeadline?: boolean;
  inByline?: boolean;
  inStory?: boolean;
  inProgram?: boolean;
  /** the term that will be queried */
  searchTerm: string;
  /** array of sources to filter by */
  sourceIds?: number[];
  /** date range start date */
  publishedStartOn: string;
  /** date range end date */
  publishedEndOn: string;
  /** sentiment min max */
  sentiment?: number[];
  /** whether content was tagged as top story or not */
  topStory?: boolean;
  /** whether content was tagged as front page or not */
  frontPage?: boolean;
  /** whether or not the content has a file associated to it or not */
  hasFile?: boolean;
  /** whether or not to bold the keywords */
  boldKeywords?: boolean;
  /** control whether unpublished content is searched */
  searchUnpublished?: boolean;
  /** papers filter */
  section?: string;
  page?: string;
  edition?: string;
  /** control which content type to be returned */
  contentTypes?: ContentTypeName[];
  /** control content to be returned based off of the contributors */
  contributorIds?: number[];
  /** control content to be returned based off of the product associated to it */
  productIds?: number[];
  /** control content to be returned based off of the series associated to it */
  seriesIds?: number[];
  /** control content to be returned based off of the tags associated to it */
  tags?: string[];
}
