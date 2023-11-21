import React from 'react';
import ReactDatePicker from 'react-datepicker';
import { FaX } from 'react-icons/fa6';
import { useContent } from 'store/hooks';
import { Row, ToggleGroup } from 'tno-core';

import { QuickPickerNames } from './constants';
import { determineActivePicker } from './utils';

export const DateSection: React.FC = () => {
  // disable quick picker when user selects a date on react-date-picker
  const [disableQuickPick, setDisableQuickPick] = React.useState(false);
  const [{ searchFilter: filter }, { storeSearchFilter: storeFilter }] = useContent();

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
          startDate={filter?.publishedStartOn ? new Date(filter.publishedStartOn) : null}
          selected={filter?.publishedStartOn ? new Date(filter.publishedStartOn) : null}
          selectsStart
          endDate={filter?.publishedEndOn ? new Date(filter.publishedEndOn) : null}
          onChange={(date) => {
            storeFilter({ ...filter, publishedStartOn: date?.toISOString() ?? '' });
            setDisableQuickPick(true);
          }}
        />
        <p>to</p>
        <ReactDatePicker
          className="date-picker"
          startDate={filter?.publishedStartOn ? new Date(filter.publishedStartOn) : null}
          selected={filter?.publishedEndOn ? new Date(filter.publishedEndOn) : null}
          selectsEnd
          endDate={filter?.publishedEndOn ? new Date(filter.publishedEndOn) : null}
          onChange={(date) => {
            setDisableQuickPick(true);
            storeFilter({ ...filter, publishedEndOn: date?.toISOString() ?? '' });
          }}
        />
        <FaX
          className="clear"
          onClick={() => {
            storeFilter({ ...filter, publishedEndOn: '', publishedStartOn: '' });
            setDisableQuickPick(false);
          }}
        />
      </Row>
      <ToggleGroup
        className="date-range-toggle"
        disabled={disableQuickPick}
        defaultSelected={determineActivePicker(filter.dateOffset ?? 0)}
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
