import React from 'react';
import { FaCalendarAlt } from 'react-icons/fa';
import { FaX } from 'react-icons/fa6';
import { useContent } from 'store/hooks';
import { Button, ButtonVariant, replaceQueryParams, SelectDate } from 'tno-core';

import * as styled from './styled';

export interface IDateRangeSectionProps {}

/**
 * Provides a component to set the date range.
 * @param param0 Component properties.
 * @returns Component.
 */
export const DateRangeSection: React.FC<IDateRangeSectionProps> = () => {
  const [{ filter, filterAdvanced }, { storeFilter, storeFilterAdvanced }] = useContent();

  return (
    <styled.DateRangeSection>
      <FaCalendarAlt className="icon-indicator" />
      <SelectDate
        className="select-date"
        name="startDate"
        placeholderText="mm/dd/yyyy"
        selected={!!filterAdvanced.startDate ? new Date(filterAdvanced.startDate) : undefined}
        width="8em"
        onChange={(date) => {
          storeFilter((filter) => ({ ...filter, timeFrame: 0 }));
          storeFilterAdvanced((filter) => ({
            ...filter,
            startDate: !!date ? date.toString() : undefined,
          }));
        }}
      />
      <span className="to-text">to</span>
      <SelectDate
        className="select-date"
        name="endDate"
        placeholderText="mm/dd/yyyy"
        selected={!!filterAdvanced.endDate ? new Date(filterAdvanced.endDate) : undefined}
        width="8em"
        onChange={(date) => {
          date?.setHours(23, 59, 59);
          storeFilterAdvanced((filter) => ({
            ...filter,
            endDate: !!date ? date.toString() : undefined,
          }));
          storeFilter((filter) => ({ ...filter, timeFrame: 0 }));
        }}
      />
      <Button
        className="clear"
        variant={ButtonVariant.secondary}
        onClick={() => {
          const values = {
            ...filterAdvanced,
            startDate: undefined,
            endDate: undefined,
          };
          storeFilterAdvanced(values);
          storeFilter((filter) => ({ ...filter, timeFrame: 0 }));
          replaceQueryParams({ ...filter, ...values, timeFrame: 0 }, { includeEmpty: false });
        }}
      >
        <FaX />
      </Button>
    </styled.DateRangeSection>
  );
};
