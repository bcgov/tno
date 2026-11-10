import moment, { Moment } from 'moment';
import React from 'react';
import ReactDatePicker from 'react-datepicker';
import { FaCalendarDay, FaCaretLeft, FaCaretRight } from 'react-icons/fa6';

import * as styled from './styled';

export interface IDateFilterProps {
  date: string | Date | Moment | undefined;
  onChangeDate?: (start: string, end: string) => void;
}

/** Custom date filter for the subscriber home page. Control the calendar state with custom button, custom styling also applied. Also allows user to navigate a day at a time via arrow buttons. */
export const DateFilter: React.FC<IDateFilterProps> = ({ date: initDate, onChangeDate }) => {
  /** control state of open calendar from outside components. i.e custom calendar button */
  const [open, setOpen] = React.useState(false);
  const [date, setDate] = React.useState(initDate);

  // /** close calendar after a date has been selected, and fetch related content */
  React.useEffect(() => {
    setOpen(false);
  }, [date, setOpen]);

  const handleDateChange = React.useCallback(
    (date: string | Date | Moment | undefined) => {
      setDate(date);
      onChangeDate?.(
        moment(date).startOf('day').toISOString(),
        moment(date).endOf('day').toISOString(),
      );
    },
    [onChangeDate],
  );

  return (
    <styled.DateFilter alignItems="center" justifyContent="center" className="date-navigator">
      <FaCaretLeft
        className="caret"
        onClick={() => handleDateChange(moment(date).add(-1, 'day'))}
      />
      <div className="date">
        <FaCalendarDay className="calendar" onClick={() => setOpen((value) => !value)} />
        <ReactDatePicker
          open={open}
          maxDate={new Date()}
          disabled
          dateFormat="dd-MMM-y"
          onChange={(e) => handleDateChange(e?.toISOString())}
          onChangeRaw={(e) => setOpen(false)}
          selected={date ? moment(date).toDate() : new Date()}
        />
      </div>
      <FaCaretRight
        className="caret"
        onClick={() => handleDateChange(moment(date).add(1, 'day'))}
      />
    </styled.DateFilter>
  );
};
