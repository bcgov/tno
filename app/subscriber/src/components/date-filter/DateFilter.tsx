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

  const handleDateChange = React.useCallback(
    ({ startDate, dateOffset }: { startDate?: string; dateOffset?: number }) => {
      let newDate;
      if (startDate && dateOffset === undefined) {
        newDate = new Date(startDate);
      } else if (startDate && dateOffset !== undefined) {
        newDate = new Date(startDate);
        newDate.setDate(newDate.getDate() + dateOffset);
      }
      storeFilter({
        ...filter,
        startDate: moment(newDate).startOf('day').toISOString(),
        endDate: moment(newDate).endOf('day').toISOString(),
      });
      onChangeDate?.();
    },
    [filter, onChangeDate, storeFilter],
  );

  return (
    <styled.DateFilter alignItems="center" justifyContent="center" className="date-navigator">
      <FaCaretLeft
        className="caret"
        onClick={() =>
          handleDateChange({
            startDate: filter.startDate ? filter.startDate : new Date().toISOString(),
            dateOffset: -1,
          })
        }
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
        onClick={() =>
          handleDateChange({
            startDate: filter.startDate ? filter.startDate : new Date().toISOString(),
            dateOffset: 1,
          })
        }
      />
    </styled.DateFilter>
  );
};
