import { useFormikContext } from 'formik';
import React from 'react';
import { useAdminStore } from 'store/slices';
import { Checkbox, IconButton, IReportModel, IUserFilter, Row, Text } from 'tno-core';

interface IAdminFilterProps {
  onFilterChange?: (value: IUserFilter) => void;
  onSearch?: (value?: IUserFilter) => void;
}

export const ReportSubscriberFilter: React.FC<IAdminFilterProps> = ({
  onFilterChange,
  onSearch,
}) => {
  const { values } = useFormikContext<IReportModel>();
  const [{ reportSubscriberFilter }, { storeReportSubscriberFilter }] = useAdminStore();

  return (
    <Row className="filter-bar" justifyContent="center">
      <Text
        onChange={(e) => {
          const filter = { ...reportSubscriberFilter, keyword: e.target.value };
          storeReportSubscriberFilter(filter);
          onFilterChange?.(filter);
        }}
        onKeyUp={(e) => {
          if (e.code === 'Enter') onSearch?.(reportSubscriberFilter);
        }}
        placeholder="Search by keyword"
        name="search"
        value={reportSubscriberFilter.keyword}
      ></Text>
      <Checkbox
        name="isSubscribed"
        label="Is subscribed"
        checked={!!reportSubscriberFilter.isSubscribedToReportId}
        onChange={(e) => {
          const filter = {
            ...reportSubscriberFilter,
            isSubscribedToReportId: e.target.checked ? values.id : undefined,
          };
          storeReportSubscriberFilter(filter);
          onFilterChange?.(filter);
          onSearch?.(filter);
        }}
      />
      {!!onSearch && (
        <IconButton
          iconType="search"
          onClick={() => {
            onSearch?.(reportSubscriberFilter);
          }}
        />
      )}
      <IconButton
        iconType="reset"
        onClick={() => {
          storeReportSubscriberFilter({});
          onFilterChange?.({});
          onSearch?.();
        }}
      />
    </Row>
  );
};
