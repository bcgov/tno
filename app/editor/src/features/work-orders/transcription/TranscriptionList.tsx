import { FormPage } from 'components/formpage';
import { useTab } from 'components/tab-control';
import React from 'react';
import { useApiHub, useWorkOrders } from 'store/hooks';
import {
  CellDate,
  CellEllipsis,
  defaultPage,
  Grid,
  IGridHeaderColumnProps,
  IWorkOrderFilter,
  IWorkOrderMessageModel,
  IWorkOrderModel,
  MessageTargetKey,
  Page,
  SortDirection,
  WorkOrderTypeName,
} from 'tno-core';

import { WorkOrderFilter } from '../WorkOrderFilter';
import * as styled from './styled';
import { WorkOrderActions } from './WorkOrderActions';
import { WorkOrderStatus } from './WorkOrderStatus';

export const TranscriptionList: React.FC = () => {
  const [
    { transcriptFilter: filter },
    { findWorkOrders, updateWorkOrder, storeTranscriptFilter: storeFilter },
  ] = useWorkOrders();
  const hub = useApiHub();
  const { navigate } = useTab();

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

  hub.useHubEffect(MessageTargetKey.WorkOrder, onWorkOrder);

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

  const handlePageChange = React.useCallback(
    (page: number) => {
      if (filter.page !== page) {
        const newFilter = {
          ...filter,
          page: page,
        };
        storeFilter(newFilter);
      }
    },
    [filter, storeFilter],
  );

  const handleQuantityChange = React.useCallback(
    (quantity: number) => {
      if (filter.quantity !== quantity) {
        const newFilter = {
          ...filter,
          quantity: quantity,
        };
        storeFilter(newFilter);
      }
    },
    [filter, storeFilter],
  );

  const handleSortChange = React.useCallback(
    (column: IGridHeaderColumnProps, direction: SortDirection) => {
      if (column.name) {
        const sort =
          direction === SortDirection.Descending ? [`${column.name} desc`] : [column.name];

        storeFilter({ ...filter, sort });
      }
    },
    [filter, storeFilter],
  );

  return (
    <styled.TranscriptionList>
      <FormPage>
        <WorkOrderFilter filter={filter} onFilterChange={(filter) => storeFilter(filter)} />
        <Grid
          items={page.items}
          pageIndex={page.pageIndex}
          itemsPerPage={page.pageSize}
          totalItems={page.total}
          isOneBasedIndexing={true}
          showPaging
          onNavigatePage={async (page) => {
            handlePageChange(page);
          }}
          onQuantityChange={async (quantity) => {
            handleQuantityChange(quantity);
          }}
          onSortChange={async (column, direction) => {
            handleSortChange(column, direction);
          }}
          renderHeader={() => [
            { name: 'configuration.headline', label: 'Headline', size: '40%', sortable: false },
            { name: 'content.otherSource', label: 'Source', size: '14%', sortable: true },
            { name: 'content.mediaType', label: 'Media Type', size: '13%', sortable: true },
            { name: 'requestor.username', label: 'Requested By', size: '11%', sortable: true },
            { name: 'createdOn', label: 'Created', size: '12%', sortable: true },
            { name: 'status', label: 'Status', size: '6%', sortable: true },
            { name: '', label: '', sortable: true },
          ]}
          renderColumns={(row: IWorkOrderModel, rowIndex) => {
            return [
              {
                column: (
                  <div
                    key="1"
                    className="clickable"
                    onClick={() => navigate(row.contentId ?? 0, '/contents')}
                  >
                    <CellEllipsis>{row.configuration.headline}</CellEllipsis>
                  </div>
                ),
              },
              {
                column: (
                  <div
                    key="2"
                    className="clickable"
                    onClick={() => navigate(row.contentId ?? 0, '/contents')}
                  >
                    <CellEllipsis>{row.content?.otherSource}</CellEllipsis>
                  </div>
                ),
              },
              {
                column: (
                  <div
                    key="3"
                    className="clickable"
                    onClick={() => navigate(row.contentId ?? 0, '/contents')}
                  >
                    <CellEllipsis>{row.content?.mediaType}</CellEllipsis>
                  </div>
                ),
              },
              {
                column: (
                  <div
                    key="4"
                    className="clickable"
                    onClick={() => navigate(row.contentId ?? 0, '/contents')}
                  >
                    <CellEllipsis>{row.requestor?.username}</CellEllipsis>
                  </div>
                ),
              },
              {
                column: (
                  <div
                    key="5"
                    className="clickable"
                    onClick={() => navigate(row.contentId ?? 0, '/contents')}
                  >
                    <CellDate value={row.createdOn} />
                  </div>
                ),
              },
              {
                column: (
                  <div
                    key="6"
                    className="clickable"
                    onClick={() => navigate(row.contentId ?? 0, '/contents')}
                  >
                    <CellEllipsis>
                      <WorkOrderStatus row={row} />
                    </CellEllipsis>
                  </div>
                ),
              },
              {
                column: (
                  <div key="7">
                    <CellEllipsis>
                      <WorkOrderActions row={row} onCancel={handleCancel} />
                    </CellEllipsis>
                  </div>
                ),
              },
            ];
          }}
        />
      </FormPage>
    </styled.TranscriptionList>
  );
};

export default TranscriptionList;
