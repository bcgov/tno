import React from 'react';
import ReactDatePicker from 'react-datepicker';
import { FaCalendarDay, FaCaretLeft, FaCaretRight } from 'react-icons/fa6';

import { dayjs, type Dayjs } from '@/shared/utils/date';

import * as styled from './styled';

/**
 * The date is changed with the calendar button and the arrows, not by typing. react-datepicker 9
 * disables day selection when the picker is disabled or read-only, so only the text box is made
 * inert.
 */
const InertInput = React.forwardRef<HTMLInputElement, React.InputHTMLAttributes<HTMLInputElement>>(
  (props, ref) => <input {...props} ref={ref} readOnly tabIndex={-1} />,
);

export interface IDateFilterProps {
  date: string | Date | Dayjs | undefined;
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
    (date: string | Date | Dayjs | undefined) => {
      setDate(date);
      onChangeDate?.(
        dayjs(date).startOf('day').toISOString(),
        dayjs(date).endOf('day').toISOString(),
      );
    },
    [onChangeDate],
  );

  return (
    <styled.DateFilter alignItems="center" justifyContent="center" className="date-navigator">
      <FaCaretLeft className="caret" onClick={() => handleDateChange(dayjs(date).add(-1, 'day'))} />
      <div className="date">
        <FaCalendarDay className="calendar" onClick={() => setOpen((value) => !value)} />
        <ReactDatePicker
          open={open}
          maxDate={new Date()}
          customInput={<InertInput />}
          dateFormat="dd-MMM-y"
          onChange={(e: Date | null) => handleDateChange(e?.toISOString())}
          onChangeRaw={(e) => setOpen(false)}
          selected={date ? dayjs(date).toDate() : new Date()}
        />
      </div>
      <FaCaretRight className="caret" onClick={() => handleDateChange(dayjs(date).add(1, 'day'))} />
    </styled.DateFilter>
  );
};
