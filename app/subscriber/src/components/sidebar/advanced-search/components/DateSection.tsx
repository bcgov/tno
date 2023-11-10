import ReactDatePicker from 'react-datepicker';
import { Row, Show } from 'tno-core';

import { defaultAdvancedSearch } from '../constants';
import { IAdvancedSearchFilter } from '../interfaces';

export interface IDateSectionProps {
  /** variable that keeps track of whether the sub-menu is expanded or not */
  dateExpanded: boolean;
  /** function that will update the startOn/endOn for the advanced filter */
  setAdvancedSearch: (advancedSearch: IAdvancedSearchFilter) => void;
  /** advanced search object, may start as undefined if nothing is set */
  advancedSearch: IAdvancedSearchFilter;
}

export const DateSection: React.FC<IDateSectionProps> = ({
  dateExpanded,
  advancedSearch,
  setAdvancedSearch,
}) => {
  return (
    <Show visible={dateExpanded}>
      <Row className="expanded date-range">
        <ReactDatePicker
          className="date-picker"
          startDate={
            advancedSearch?.startDate
              ? new Date(advancedSearch.startDate)
              : new Date(defaultAdvancedSearch.startDate)
          }
          selected={
            advancedSearch?.startDate
              ? new Date(advancedSearch.startDate)
              : new Date(defaultAdvancedSearch.startDate)
          }
          selectsStart
          endDate={
            advancedSearch?.endDate
              ? new Date(advancedSearch.endDate)
              : new Date(defaultAdvancedSearch.endDate)
          }
          onChange={(date) =>
            setAdvancedSearch({ ...advancedSearch, startDate: date?.toISOString() ?? '' })
          }
        />
        <p>to</p>
        <ReactDatePicker
          className="date-picker"
          startDate={
            advancedSearch?.startDate
              ? new Date(advancedSearch.startDate)
              : new Date(defaultAdvancedSearch.startDate)
          }
          selected={
            advancedSearch?.endDate
              ? new Date(advancedSearch.endDate)
              : new Date(defaultAdvancedSearch.endDate)
          }
          selectsEnd
          endDate={
            advancedSearch?.endDate
              ? new Date(advancedSearch.endDate)
              : new Date(defaultAdvancedSearch.endDate)
          }
          onChange={(date) =>
            setAdvancedSearch({ ...advancedSearch, endDate: date?.toISOString() ?? '' })
          }
        />
      </Row>
    </Show>
  );
};
