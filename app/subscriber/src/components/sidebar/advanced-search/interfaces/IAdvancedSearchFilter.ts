export interface IAdvancedSearchFilter {
  /** array of actions to filter by */
  actions?: string[];
  /** keeps track of which field the search term will query */
  inHeadline?: boolean;
  inByline?: boolean;
  inStory?: boolean;
  /** the term that will be queried */
  searchTerm: string;
  /** array of sources to filter by */
  sourceIds?: number[];
  /** date range start date */
  startDate: string;
  /** date range end date */
  endDate: string;
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
  /** index to control whether content is published or unpublished */
  useUnpublished?: boolean;
}
