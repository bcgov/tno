export interface IAdvancedSearchFilter {
  /** keeps track of which field the search term will query */
  searchInField: string;
  /** the term that will be queried */
  searchTerm: string;
  /** array of sources to filter by */
  sourceIds?: number[];
  /** date range start date */
  startDate: string;
  /** date range end date */
  endDate: string;
}
