import { FormPage } from 'components/formpage';
import React from 'react';
import { useApiHub, useWorkOrders } from 'store/hooks';
import {
  defaultPage,
  FlexboxTable,
  ITableInternal,
  ITablePage,
  ITableSort,
  IWorkOrderFilter,
  IWorkOrderMessageModel,
  IWorkOrderModel,
  MessageTargetName,
  Page,
  WorkOrderTypeName,
} from 'tno-core';

import { WorkOrderFilter } from '../WorkOrderFilter';
import { useColumns } from './hooks';
import * as styled from './styled';

export const TranscriptionList: React.FC = () => {
  const [
    { transcriptFilter: filter },
    { findWorkOrders, updateWorkOrder, storeTranscriptFilter: storeFilter },
  ] = useWorkOrders();
  const hub = useApiHub();

  const [page, setPage] = React.useState(defaultPage<IWorkOrderModel>());

  const fetch = React.useCallback(
    async (filter: IWorkOrderFilter) => {
      const response = await findWorkOrders(filter);
      setPage(
        new Page(
          response.data.page - 1,
          response.data.quantity,
          response.data.items,
          response.data.total,
        ),
      );
    },
    [findWorkOrders],
  );

  React.useEffect(() => {
    fetch(filter).catch(() => {});
    // Only fetch when filter changes.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [filter]);

  const onWorkOrder = React.useCallback(
    async (workOrder: IWorkOrderMessageModel) => {
      if ([WorkOrderTypeName.Transcription].includes(workOrder.workType)) {
        setPage({
          ...page,
          items: page.items.map((i) =>
            i.id !== workOrder.id ? i : { ...i, status: workOrder.status },
          ),
        });
      }
    },
    [page],
  );

  hub.useHubEffect(MessageTargetName.WorkOrder, onWorkOrder);

  const handleCancel = React.useCallback(
    async (workOrder: IWorkOrderModel) => {
      try {
        const response = await updateWorkOrder(workOrder);
        setPage({
          ...page,
          items: page.items.map((i) => (i.id !== workOrder.id ? i : response.data)),
        });
      } catch {}
    },
    [page, updateWorkOrder],
  );

  const columns = useColumns({ onCancel: handleCancel });

  const handlePageChange = (page: ITablePage, table: ITableInternal<IWorkOrderModel>) => {
    storeFilter({ ...filter, page: page.pageIndex + 1, quantity: page.pageSize });
  };

  const handleSortChange = (
    sort: ITableSort<IWorkOrderModel>[],
    table: ITableInternal<IWorkOrderModel>,
  ) => {
    storeFilter({
      ...filter,
      sort: sort.length
        ? sort.filter((s) => s.isSorted).map((s) => `${s.sort}${s.isSortedDesc ? ' desc' : ''}`)
        : [],
    });
  };

  return (
    <styled.TranscriptionList>
      <FormPage>
        <WorkOrderFilter filter={filter} onFilterChange={(filter) => storeFilter(filter)} />
        <FlexboxTable
          rowId={'id'}
          data={page.items}
          columns={columns}
          manualPaging={true}
          pageIndex={(filter.page ?? 1) - 1}
          pageSize={filter.quantity ?? 100}
          pageCount={page.pageCount}
          showSort={true}
          onPageChange={handlePageChange}
          onSortChange={handleSortChange}
          showActive={false}
        />
      </FormPage>
    </styled.TranscriptionList>
  );
};

export default TranscriptionList;
