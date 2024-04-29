import moment from 'moment';
import React from 'react';
import ReactDatePicker from 'react-datepicker';
import { FaCalendarDay, FaCaretLeft, FaCaretRight } from 'react-icons/fa6';
import { IFilterSettingsModel } from 'tno-core';

import * as styled from './styled';

export interface IDateFilterProps {
  loaded?: boolean;
  filter: IFilterSettingsModel;
  storeFilter: (filter: IFilterSettingsModel) => void;
  onChangeDate?: () => void;
}

/** Custom date filter for the subscriber home page. Control the calendar state with custom button, custom styling also applied. Also allows user to navigate a day at a time via arrow buttons. */
export const DateFilter: React.FC<IDateFilterProps> = ({
  loaded,
  filter,
  storeFilter,
  onChangeDate,
}) => {
  /** control state of open calendar from outside components. i.e custom calendar button */
  const [open, setOpen] = React.useState(false);

  // /** close calendar after a date has been selected, and fetch related content */
  React.useEffect(() => {
    setOpen(false);
  }, [filter.startDate, setOpen]);

  // set initial DateFilter date
  React.useEffect(() => {
    if (!filter.startDate) {
      const todaysDate = moment().toISOString();
      handleDateChange({ startDate: todaysDate });
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [filter]);

  /** update state variable when date changes, can be controlled via date picker or arrows */
  React.useEffect(() => {
    if (filter.startDate) {
      handleDateChange({ startDate: filter.startDate });
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [filter.startDate]);

  const handleDateChange = ({
    startDate,
    dateOffset,
  }: {
    startDate?: string;
    dateOffset?: number;
  }) => {
    // must have at least an initial filter.startDate to go off, or an explicit startDate passed in
    if (!filter.startDate && !startDate) return;

    let newDate;
    if (startDate) {
      newDate = new Date(startDate);
    } else if (filter.startDate && dateOffset) {
      newDate = new Date(filter.startDate);
      newDate.setDate(newDate.getDate() + dateOffset);
    }

    // only store filter there's no initial startDate yet, or if the new date differs from stored date
    if (!filter.startDate || newDate?.toISOString() !== new Date(filter.startDate).toISOString()) {
      storeFilter({
        ...filter,
        startDate: moment(newDate).startOf('day').toISOString(),
        endDate: moment(newDate).endOf('day').toISOString(),
      });
    }

    if (onChangeDate) {
      onChangeDate();
    }
  };

  return (
    <styled.DateFilter alignItems="center" justifyContent="center" className="date-navigator">
      <FaCaretLeft className="caret" onClick={() => handleDateChange({ dateOffset: -1 })} />
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
      <FaCaretRight className="caret" onClick={() => handleDateChange({ dateOffset: 1 })} />
    </styled.DateFilter>
  );
};
