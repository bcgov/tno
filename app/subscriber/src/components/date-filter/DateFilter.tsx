import moment from 'moment';
import React from 'react';
import ReactDatePicker from 'react-datepicker';
import { FaCalendarDay, FaCaretLeft, FaCaretRight } from 'react-icons/fa6';
import { useContent } from 'store/hooks';
import { IFilterSettingsModel } from 'tno-core';

import * as styled from './styled';

export interface IDateFilterProps {
  loaded?: boolean;
  filter: IFilterSettingsModel;
  storeFilter: (filter: IFilterSettingsModel) => void;
}

/** Custom date filter for the subscriber home page. Control the calendar state with custom button, custom styling also applied. Also allows user to navigate a day at a time via arrow buttons. */
export const DateFilter: React.FC<IDateFilterProps> = ({ loaded, filter, storeFilter }) => {
  /** control state of open calendar from outside components. i.e custom calendar button */
  const [open, setOpen] = React.useState(false);

  // /** close calendar after a date has been selected, and fetch related content */
  React.useEffect(() => {
    setOpen(false);
  }, [filter.startDate, setOpen]);

  // set initial DateFilter date
  React.useEffect(() => {
    if (!filter.startDate) {
      storeFilter({
        ...filter,
        startDate: moment().startOf('day').toISOString(),
        endDate: moment().endOf('day').toISOString(),
      });
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [filter]);

  /** update state variable when date changes, can be controlled via date picker or arrows */
  React.useEffect(() => {
    if (filter.startDate) {
      handleDateChange({ startDate: filter.startDate });
    }
    // only want the above to trigger when date changes, not when filterAdvanced changes
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [filter.startDate]);

  const handleDateChange = ({
    startDate,
    days,
    direction,
  }: {
    startDate?: string;
    days?: number;
    direction?: 'forwards' | 'backwards';
  }) => {
    if (!filter.startDate) return;
    let newDate;
    if (startDate) {
      newDate = new Date(startDate);
    } else if (days && direction) {
      newDate = new Date(filter.startDate);
      if (direction === 'backwards') newDate.setDate(newDate.getDate() - days);
      if (direction === 'forwards') newDate.setDate(newDate.getDate() + days);
    }
    // only store filter if the new date differs from stored date
    if (newDate?.toISOString() !== filter.startDate) {
      storeFilter({
        ...filter,
        startDate: moment(newDate).startOf('day').toISOString(),
        endDate: moment(newDate).endOf('day').toISOString(),
      });
    }
  };

  return (
    <styled.DateFilter alignItems="center" justifyContent="center" className="date-navigator">
      <FaCaretLeft
        className="caret"
        onClick={() => handleDateChange({ days: 1, direction: 'backwards' })}
      />
      <div className="date">
        <FaCalendarDay className="calendar" onClick={() => setOpen(true)} />
        <ReactDatePicker
          open={open}
          maxDate={new Date()}
          disabled
          dateFormat="dd-MMM-y"
          onChange={(e) => handleDateChange({ startDate: e?.toISOString() })}
          selected={!!filter.startDate ? new Date(filter.startDate) : new Date()}
        />
      </div>
      <FaCaretRight
        className="caret"
        onClick={() => handleDateChange({ days: 1, direction: 'forwards' })}
      />
    </styled.DateFilter>
  );
};
