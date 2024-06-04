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

  const offset =
    filter?.startDate || filter?.endDate
      ? undefined
      : determineActivePicker(filter.dateOffset ?? 0);

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
              dateOffset: undefined,
            });
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
            storeFilter({
              ...filter,
              endDate: date ? moment(date)?.endOf('day').toISOString() : '',
              dateOffset: undefined,
            });
          }}
        />
        <FaX
          className="clear"
          onClick={() => {
            storeFilter({ ...filter, endDate: '', startDate: '', dateOffset: 0 });
          }}
        />
      </Row>
      <ToggleGroup
        className="date-range-toggle"
        defaultSelected={offset}
        options={[
          {
            label: QuickPickerNames.Today,
            onClick: () =>
              storeFilter({ ...filter, dateOffset: 0, startDate: undefined, endDate: undefined }),
          },
          {
            label: QuickPickerNames.TwentyFourHours,
            onClick: () =>
              storeFilter({ ...filter, dateOffset: 1, startDate: undefined, endDate: undefined }),
          },
          {
            label: QuickPickerNames.FortyEightHours,
            onClick: () =>
              storeFilter({ ...filter, dateOffset: 2, startDate: undefined, endDate: undefined }),
          },
          {
            label: QuickPickerNames.SevenDays,
            onClick: () =>
              storeFilter({ ...filter, dateOffset: 7, startDate: undefined, endDate: undefined }),
          },
          {
            label: QuickPickerNames.OneMonth,
            onClick: () =>
              storeFilter({ ...filter, dateOffset: 30, startDate: undefined, endDate: undefined }),
          },
          {
            label: QuickPickerNames.TwoMonths,
            onClick: () =>
              storeFilter({ ...filter, dateOffset: 60, startDate: undefined, endDate: undefined }),
          },
          {
            label: QuickPickerNames.ThreeMonths,
            onClick: () =>
              storeFilter({ ...filter, dateOffset: 90, startDate: undefined, endDate: undefined }),
          },
          {
            label: QuickPickerNames.SixMonths,
            onClick: () =>
              storeFilter({ ...filter, dateOffset: 120, startDate: undefined, endDate: undefined }),
          },
        ]}
        activeColor="#6750a4"
      />
    </Row>
  );
};
