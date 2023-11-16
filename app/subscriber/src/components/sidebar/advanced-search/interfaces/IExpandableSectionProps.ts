import { IAdvancedSearchFilter } from './IAdvancedSearchFilter';

export interface IExpandedSectionProps {
  /** change the state of the advanced search */
  setAdvancedSearch: (advancedSearch: IAdvancedSearchFilter) => void;
  /** use the current state of advanced search */
  advancedSearch: IAdvancedSearchFilter;
}
