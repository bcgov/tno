import moment from 'moment';
import React from 'react';
import ReactDatePicker from 'react-datepicker';
import { FaCalendarDay, FaCaretLeft, FaCaretRight } from 'react-icons/fa6';
import { IFilterSettingsModel } from 'tno-core';

import * as styled from './styled';

export interface IDateFilterProps {
  filter: IFilterSettingsModel;
  storeFilter: (filter: IFilterSettingsModel) => void;
}

/** Custom date filter for the subscriber home page. Control the calendar state with custom button, custom styling also applied. Also allows user to navigate a day at a time via arrow buttons. */
export const DateFilter: React.FC<IDateFilterProps> = ({ filter, storeFilter }) => {
  /** control state of open calendar from outside components. i.e custom calendar button */
  const [open, setOpen] = React.useState(false);

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
  // /** close calendar after a date has been selected, and fetch related content */
  React.useEffect(() => {
    setOpen(false);
  }, [filter.startDate, setOpen]);

  /** update state variable when date changes, can be controlled via date picker or arrows */
  React.useEffect(() => {
    storeFilter({
      ...filter,
      startDate: moment(filter.startDate).startOf('day').toISOString(),
      endDate: moment(filter.startDate).endOf('day').toISOString(),
    });
    // only want the above to trigger when date changes, not when filterAdvanced changes
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [filter.startDate]);

  /** function to help manipulate the current date based on user input */
  const adjustDate = (days: number, direction: 'forwards' | 'backwards') => {
    if (!filter.startDate) return;
    let tempDate = new Date(filter.startDate);
    if (direction === 'backwards') tempDate.setDate(tempDate.getDate() - days);
    if (direction === 'forwards') tempDate.setDate(tempDate.getDate() + days);
    storeFilter({
      ...filter,
      startDate: moment(tempDate).startOf('day').toISOString(),
      endDate: moment(tempDate).endOf('day').toISOString(),
    });
  };

  return (
    <styled.DateFilter alignItems="center" justifyContent="center" className="date-navigator">
      <FaCaretLeft className="caret" onClick={() => adjustDate(1, 'backwards')} />
      <div className="date">
        <FaCalendarDay className="calendar" onClick={() => setOpen(true)} />
        <ReactDatePicker
          open={open}
          maxDate={new Date()}
          disabled
          dateFormat="dd-MMM-y"
          onChange={(e) => storeFilter({ ...filter, startDate: e?.toISOString() })}
          selected={!!filter.startDate ? new Date(filter.startDate) : new Date()}
        />
      </div>
      <FaCaretRight className="caret" onClick={() => adjustDate(1, 'forwards')} />
    </styled.DateFilter>
  );
};
