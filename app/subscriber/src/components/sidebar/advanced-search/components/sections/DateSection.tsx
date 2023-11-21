import moment from 'moment';
import React from 'react';
import ReactDatePicker from 'react-datepicker';
import { FaX } from 'react-icons/fa6';
import { Row, ToggleGroup } from 'tno-core';

import { defaultAdvancedSearch } from '../../constants';
import { IExpandedSectionProps } from '../../interfaces';

export const DateSection: React.FC<IExpandedSectionProps> = ({
  advancedSearch,
  setAdvancedSearch,
}) => {
  // disable quick picker when user selects a date on react-date-picker
  const [disableQuickPick, setDisableQuickPick] = React.useState(false);
  const filterDate = (range: 'today' | '24' | '48' | 'week') => {
    switch (range) {
      case 'today':
        setAdvancedSearch({
          ...advancedSearch,
          startDate: moment().startOf('day').toISOString(),
          endDate: moment().endOf('day').toISOString(),
        });
        break;
      case '24':
        setAdvancedSearch({
          ...advancedSearch,
          startDate: moment().subtract(24, 'hours').toISOString(),
          endDate: moment().toISOString(),
        });
        break;
      case '48':
        setAdvancedSearch({
          ...advancedSearch,
          startDate: moment().subtract(48, 'hours').toISOString(),
          endDate: moment().toISOString(),
        });
        break;
      case 'week':
        setAdvancedSearch({
          ...advancedSearch,
          startDate: moment().subtract(7, 'days').toISOString(),
          endDate: moment().toISOString(),
        });
        break;
      default:
        break;
    }
  };
  return (
    <Row className="expanded date-range">
      <Row className="picker">
        <ReactDatePicker
          className="date-picker"
          startDate={
            advancedSearch?.startDate
              ? new Date(advancedSearch.startDate)
              : new Date(defaultAdvancedSearch.startDate)
          }
          selected={
            advancedSearch?.startDate
              ? new Date(advancedSearch.startDate)
              : new Date(defaultAdvancedSearch.startDate)
          }
          selectsStart
          endDate={
            advancedSearch?.endDate
              ? new Date(advancedSearch.endDate)
              : new Date(defaultAdvancedSearch.endDate)
          }
          onChange={(date) => {
            setAdvancedSearch({ ...advancedSearch, startDate: date?.toISOString() ?? '' });
            setDisableQuickPick(true);
          }}
        />
        <p>to</p>
        <ReactDatePicker
          className="date-picker"
          startDate={
            advancedSearch?.startDate
              ? new Date(advancedSearch.startDate)
              : new Date(defaultAdvancedSearch.startDate)
          }
          selected={
            advancedSearch?.endDate
              ? new Date(advancedSearch.endDate)
              : new Date(defaultAdvancedSearch.endDate)
          }
          selectsEnd
          endDate={
            advancedSearch?.endDate
              ? new Date(advancedSearch.endDate)
              : new Date(defaultAdvancedSearch.endDate)
          }
          onChange={(date) => {
            setDisableQuickPick(true);
            setAdvancedSearch({ ...advancedSearch, endDate: date?.toISOString() ?? '' });
          }}
        />
        <FaX
          className="clear"
          onClick={() => {
            setAdvancedSearch({ ...advancedSearch, endDate: '', startDate: '' });
            setDisableQuickPick(false);
          }}
        />
      </Row>
      <ToggleGroup
        className="date-range-toggle"
        disabled={disableQuickPick}
        defaultSelected="7 DAYS"
        options={[
          { label: 'TODAY', onClick: () => filterDate('today') },
          { label: '24 HOURS', onClick: () => filterDate('24') },
          { label: '48 HOURS', onClick: () => filterDate('48') },
          { label: '7 DAYS', onClick: () => filterDate('week') },
        ]}
        activeColor="#6750a4"
      />
    </Row>
  );
};
