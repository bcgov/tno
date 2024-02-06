import moment from 'moment';
import React from 'react';
import ReactDatePicker from 'react-datepicker';
import { FaX } from 'react-icons/fa6';
import { useContent } from 'store/hooks';
import { Row, ToggleGroup } from 'tno-core';

import { QuickPickerNames } from './constants';
import { determineActivePicker } from './utils';

export const DateSection: React.FC = () => {
  const [
    {
      search: { filter },
    },
    { storeSearchFilter: storeFilter },
  ] = useContent();

  // disable quick picker when user selects a date on react-date-picker
  const [disableQuickPick, setDisableQuickPick] = React.useState(
    filter?.startDate || filter?.endDate ? true : false,
  );

  // ensure quick pick is disabled if there's a start or end date
  React.useEffect(() => {
    if (filter?.startDate || filter?.endDate) {
      setDisableQuickPick(true);
    } else {
      setDisableQuickPick(false);
    }
    // only run when flag changes
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [filter?.startDate, filter?.endDate]);

  // ensure that date offset is cleared when using the custom date picker
  React.useEffect(() => {
    disableQuickPick && storeFilter({ ...filter, dateOffset: undefined });
    // only run when flag changes
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [disableQuickPick]);

  return (
    <Row className="expanded date-range">
      <Row className="picker">
        <ReactDatePicker
          className="date-picker"
          // startDate must not be allowed to be AFTER the end date or AFTER today's date
          maxDate={filter?.endDate ? new Date(filter.endDate) : new Date()}
          startDate={filter?.startDate ? new Date(filter.startDate) : null}
          selected={filter?.startDate ? new Date(filter.startDate) : null}
          selectsStart
          endDate={filter?.endDate ? new Date(filter.endDate) : null}
          onChange={(date) => {
            storeFilter({
              ...filter,
              startDate: date ? moment(date)?.startOf('day').toISOString() : '',
            });
            setDisableQuickPick(true);
          }}
        />
        <p>to</p>
        <ReactDatePicker
          className="date-picker"
          // endDate must not be allowed to be AFTER today's date
          maxDate={new Date()}
          // endDate must not be allowed to be BEFORE the startDate
          minDate={!!filter.startDate ? new Date(filter.startDate) : undefined}
          startDate={filter?.startDate ? new Date(filter.startDate) : null}
          selected={filter?.endDate ? new Date(filter.endDate) : null}
          selectsEnd
          endDate={filter?.endDate ? new Date(filter.endDate) : null}
          onChange={(date) => {
            setDisableQuickPick(true);
            storeFilter({
              ...filter,
              endDate: date ? moment(date)?.endOf('day').toISOString() : '',
            });
          }}
        />
        <FaX
          className="clear"
          onClick={() => {
            storeFilter({ ...filter, endDate: '', startDate: '' });
            setDisableQuickPick(false);
          }}
        />
      </Row>
      <ToggleGroup
        className="date-range-toggle"
        disabled={disableQuickPick}
        defaultSelected={
          disableQuickPick ? undefined : determineActivePicker(filter.dateOffset ?? 0)
        }
        options={[
          {
            label: QuickPickerNames.Today,
            onClick: () => storeFilter({ ...filter, dateOffset: 0 }),
          },
          {
            label: QuickPickerNames.TwentyFourHours,
            onClick: () => storeFilter({ ...filter, dateOffset: 1 }),
          },
          {
            label: QuickPickerNames.FortyEightHours,
            onClick: () => storeFilter({ ...filter, dateOffset: 2 }),
          },
          {
            label: QuickPickerNames.SevenDays,
            onClick: () => storeFilter({ ...filter, dateOffset: 7 }),
          },
        ]}
        activeColor="#6750a4"
      />
    </Row>
  );
};
