import { FormPage } from 'components/formpage';
import { NavigateOptions, useTab } from 'components/tab-control';
import moment from 'moment';
import React from 'react';
import { FaBug, FaCheckCircle, FaClock, FaPen, FaStop } from 'react-icons/fa';
import { FaCirclePause, FaRegCircleRight } from 'react-icons/fa6';
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
  MessageTargetName,
  Page,
  Row,
  Show,
  SortDirection,
  Spinner,
  WorkOrderStatusName,
  WorkOrderTypeName,
} from 'tno-core';

import { WorkOrderFilter } from '../WorkOrderFilter';
import * as styled from './styled';

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
          direction === SortDirection.None
            ? []
            : [{ id: column.name, desc: direction === SortDirection.Descending }];
        storeFilter({ ...filter, sort });
      }
    },
    [filter, storeFilter],
  );

  const renderStatusColumn = (row: IWorkOrderModel) => {
    if (row.status === WorkOrderStatusName.InProgress)
      return <Spinner size="8px" title="Transcribing" />;
    if (row.status === WorkOrderStatusName.Completed && !row.content?.isApproved)
      return <FaRegCircleRight className="completed" title="Ready to review" />;
    if (row.status === WorkOrderStatusName.Completed && row.content?.isApproved)
      return <FaCheckCircle className="completed" title="Completed" />;
    if (row.status === WorkOrderStatusName.Submitted)
      return <FaClock className="submitted" title="Submitted" />;
    if (row.status === WorkOrderStatusName.Failed)
      return <FaBug className="failed" title="Failed" />;
    if (row.status === WorkOrderStatusName.Cancelled)
      return <FaCirclePause className="cancelled" title="Cancelled" />;
    return row.status;
  };

  const renderActionsColumn = (row: IWorkOrderModel) => {
    return (
      <Row>
        <Show visible={row.status === WorkOrderStatusName.InProgress}>
          <FaStop
            className="btn btn-link red"
            title="Cancel"
            onClick={() => handleCancel?.({ ...row, status: WorkOrderStatusName.Cancelled })}
          />
        </Show>
        <Show visible={row.status === WorkOrderStatusName.Completed && !row.content?.isApproved}>
          <FaPen
            className="btn btn-link completed"
            title="Review"
            onClick={() => navigate(row.contentId ?? 0, '/contents')}
          />
        </Show>
      </Row>
    );
  };

  return (
    <styled.TranscriptionList>
      <FormPage>
        <WorkOrderFilter filter={filter} onFilterChange={(filter) => storeFilter(filter)} />
        <Grid
          items={page.items}
          pageIndex={page.pageIndex}
          itemsPerPage={page.pageSize}
          totalItems={page.total}
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
            { name: 'configuration.headline', label: 'Headline', size: '40%', sortable: true },
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
                    key=""
                    className="clickable"
                    onClick={() => navigate(row.contentId ?? 0, '/contents')}
                  >
                    <CellEllipsis key="">{row.configuration.headline}</CellEllipsis>
                  </div>
                ),
              },
              {
                column: (
                  <div
                    key=""
                    className="clickable"
                    onClick={() => navigate(row.contentId ?? 0, '/contents')}
                  >
                    <CellEllipsis key="">{row.content?.otherSource}</CellEllipsis>
                  </div>
                ),
              },
              {
                column: (
                  <div
                    key=""
                    className="clickable"
                    onClick={() => navigate(row.contentId ?? 0, '/contents')}
                  >
                    <CellEllipsis key="">{row.content?.mediaType}</CellEllipsis>
                  </div>
                ),
              },
              {
                column: (
                  <div
                    key=""
                    className="clickable"
                    onClick={() => navigate(row.contentId ?? 0, '/contents')}
                  >
                    <CellEllipsis key="">{row.requestor?.username}</CellEllipsis>
                  </div>
                ),
              },
              {
                column: (
                  <div
                    key=""
                    className="clickable"
                    onClick={() => navigate(row.contentId ?? 0, '/contents')}
                  >
                    <CellDate value={row.createdOn} />
                  </div>
                ),
              },
              {
                column: (
                  <div key="" onClick={() => navigate(row.contentId ?? 0, '/contents')}>
                    <CellEllipsis key="">{renderStatusColumn(row)}</CellEllipsis>
                  </div>
                ),
              },
              {
                column: (
                  <div key="">
                    <CellEllipsis key="">{renderActionsColumn(row)}</CellEllipsis>
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
