import { WorkOrderStatusName, WorkOrderTypeName } from 'hooks';
import React, { useState } from 'react';
import { useWorkOrders } from 'store/hooks/admin';
import { FieldSize, IconButton, Select, Text } from 'tno-core';
import { Row } from 'tno-core/dist/components/flex';
import { getEnumStringOptions } from 'utils';

import { IWorkOrderListFilter } from './interfaces/IWorkOrderListFilter';
import * as styled from './styled';

interface IWorkOrderFilterProps {}

/**
 * Provides a component to filter work orders.
 * @returns Component
 */
export const WorkOrderListFilter: React.FC<IWorkOrderFilterProps> = () => {
  const [{ workOrderFilter }, { storeFilter }] = useWorkOrders();
  const [filter, setFilter] = useState<IWorkOrderListFilter>(workOrderFilter);
  const statusOptions = getEnumStringOptions(WorkOrderStatusName);
  const typeOptions = getEnumStringOptions(WorkOrderTypeName);

  /** Handle enter key pressed for workOrder filter */
  React.useEffect(() => {
    const keyDownHandler = (event: KeyboardEvent) => {
      if (event.key === 'Enter') {
        event.preventDefault();
        storeFilter(filter);
      }
    };

    document.addEventListener('keydown', keyDownHandler);

    return () => {
      document.removeEventListener('keydown', keyDownHandler);
    };
  }, [filter, storeFilter]);

  return (
    <styled.WorkOrderListFilter>
      <Row className="filter-bar" justifyContent="center">
        <Select
          name="workType"
          options={typeOptions}
          placeholder="Search by type"
          value={typeOptions.find((s) => s.value === filter.workType) || ''}
          width={FieldSize.Medium}
          clearValue=""
          onChange={(e: any) => {
            setFilter({ ...filter, workType: e.value });
          }}
        />
        <Select
          name="status"
          options={statusOptions}
          placeholder="Search by status"
          value={statusOptions.find((s) => s.value === filter.status) || ''}
          width={FieldSize.Medium}
          clearValue=""
          onChange={(e: any) => {
            setFilter({ ...filter, status: e.value ?? '' });
          }}
        />
        <Text
          onChange={(e) => {
            setFilter({ ...filter, keyword: e.target.value });
          }}
          placeholder="Search by keyword"
          name="keyword"
          value={filter.keyword}
        />
        <IconButton
          iconType="search"
          onClick={() => {
            storeFilter(filter);
          }}
        />
        <IconButton
          iconType="reset"
          onClick={() => {
            setFilter({
              sort: [],
              keyword: '',
              status: '',
              workType: '',
              pageIndex: 0,
              pageSize: 10,
            });
            storeFilter({
              sort: [],
              pageIndex: 0,
              pageSize: 10,
              status: '',
              workType: '',
              keyword: '',
            });
          }}
        />
      </Row>
    </styled.WorkOrderListFilter>
  );
};
