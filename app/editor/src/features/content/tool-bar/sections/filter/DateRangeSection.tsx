import {
  IContentListAdvancedFilter,
  IContentListFilter,
} from 'features/content/list-view/interfaces';
import React from 'react';
import { FaCalendarAlt } from 'react-icons/fa';
import { useContent } from 'store/hooks';
import { SelectDate } from 'tno-core';

import * as styled from './styled';

export interface IDateRangeSectionProps {
  onAdvancedFilterChange: (filter: IContentListAdvancedFilter) => void;
  onChange: (filter: IContentListFilter) => void;
  onSearch: (filter: IContentListFilter & IContentListAdvancedFilter) => void;
}
export const DateRangeSection: React.FC<IDateRangeSectionProps> = ({
  onAdvancedFilterChange,
  onSearch,
  onChange,
}) => {
  const [{ filter, filterAdvanced }] = useContent();
  /** retrigger fetch on change of date or clear*/
  React.useEffect(() => {
    onSearch({ ...filter, pageIndex: 0, ...filterAdvanced });
  }, [filter, filterAdvanced, onSearch]);

  return (
    <styled.DateRangeSection>
      <FaCalendarAlt
        data-for="main-tooltip"
        data-tip="Date range"
        className="action-icon calendar"
      />
      <SelectDate
        name="startDate"
        placeholderText="mm/dd/yyyy"
        selected={!!filterAdvanced.startDate ? new Date(filterAdvanced.startDate) : undefined}
        width="8em"
        onChange={(date) =>
          onAdvancedFilterChange({
            ...filterAdvanced,
            startDate: !!date ? date.toString() : undefined,
          })
        }
      />
      <span className="to-text">to</span>
      <SelectDate
        name="endDate"
        placeholderText="mm/dd/yyyy"
        selected={!!filterAdvanced.endDate ? new Date(filterAdvanced.endDate) : undefined}
        width="8em"
        onChange={(date) => {
          date?.setHours(23, 59, 59);
          onAdvancedFilterChange({
            ...filterAdvanced,
            endDate: !!date ? date.toString() : undefined,
          });
        }}
      />
      <span
        className="clear"
        onClick={() => {
          onAdvancedFilterChange({
            ...filterAdvanced,
            startDate: undefined,
            endDate: undefined,
          });
          onChange({ ...filter, timeFrame: 0 });
        }}
      >
        X
      </span>
    </styled.DateRangeSection>
  );
};
