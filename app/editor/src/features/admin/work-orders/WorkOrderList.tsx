import isEqual from 'lodash/isEqual';
import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useWorkOrders } from 'store/hooks/admin/useWorkOrders';
import {
  CellDate,
  CellEllipsis,
  Col,
  Grid,
  IGridHeaderColumnProps,
  IWorkOrderModel,
  Page,
  Row,
  SortDirection,
} from 'tno-core';

import { IWorkOrderListFilter } from './interfaces/IWorkOrderListFilter';
import * as styled from './styled';
import { makeWorkOrderFilter } from './utils/makeWorkOrderFilter';
import { WorkOrderListFilter } from './WorkOrderListFilter';

/**
 * Provides a component for listing and filtering work orders.
 * @returns Component for listing and filtering work orders.
 */
const WorkOrderList = () => {
  const navigate = useNavigate();
  const [{ workOrders, workOrderFilter }, { findWorkOrders, storeFilter }] = useWorkOrders();

  const [page, setPage] = React.useState(
    new Page(workOrders.page - 1, workOrders.quantity, workOrders.items, workOrders.total),
  );
  const [filter, setFilter] = React.useState<IWorkOrderListFilter>();

  const handleSortChange = React.useCallback(
    (column: IGridHeaderColumnProps, direction: SortDirection) => {
      if (column.name) {
        const sort =
          direction === SortDirection.None
            ? []
            : [{ id: column.name, desc: direction === SortDirection.Descending }];
        storeFilter({ ...workOrderFilter, sort });
      }
    },
    [workOrderFilter, storeFilter],
  );

  const handleQuantityChange = React.useCallback(
    (quantity: number) => {
      if (workOrderFilter.pageSize !== quantity) {
        const newFilter = {
          ...workOrderFilter,
          pageSize: quantity,
        };
        storeFilter(newFilter);
      }
    },
    [workOrderFilter, storeFilter],
  );

  const handlePageChange = React.useCallback(
    (page: number) => {
      if (workOrderFilter.pageIndex !== page - 1) {
        const newFilter = {
          ...workOrderFilter,
          pageIndex: page - 1,
        };
        storeFilter(newFilter);
      }
    },
    [workOrderFilter, storeFilter],
  );

  const fetch = React.useCallback(
    async (filter: IWorkOrderListFilter) => {
      try {
        const data = await findWorkOrders(makeWorkOrderFilter(filter));
        const page = new Page(data.page - 1, data.quantity, data?.items, data.total);

        setPage(page);
        return page;
      } catch (error) {
        // TODO: Handle error
        throw error;
      }
    },
    [findWorkOrders],
  );

  React.useEffect(() => {
    if (!isEqual(workOrderFilter, filter)) {
      setFilter(workOrderFilter);
      fetch(workOrderFilter);
    }
  }, [fetch, filter, workOrderFilter]);

  return (
    <styled.WorkOrderList>
      <Row className="add-media" justifyContent="flex-end">
        <Col flex="1 1 0">
          A work order is a request submitted to the system (i.e. transcription, NLP).
        </Col>
      </Row>
      <WorkOrderListFilter />
      <Grid
        items={page.items}
        pageIndex={page.pageIndex - 1}
        itemsPerPage={workOrderFilter.pageSize}
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
          { name: 'workType', label: 'Type', size: '15%', sortable: true },
          { name: 'configuration', label: 'Content', size: '40%', sortable: true },
          { name: 'createdOn', label: 'Submitted', size: '20%', sortable: true },
          { name: 'updatedOn', label: 'Updated', size: '15%', sortable: true },
          { name: 'status', label: 'Status', size: '10%', sortable: true },
        ]}
        renderColumns={(row: IWorkOrderModel, rowIndex) => {
          return [
            {
              column: (
                <div key="" className="clickable" onClick={() => navigate(`${row.id}`)}>
                  <CellEllipsis>{row.workType}</CellEllipsis>
                </div>
              ),
            },
            {
              column: (
                <div key="" className="clickable" onClick={() => navigate(`${row.id}`)}>
                  <CellEllipsis>{row.configuration?.headline}</CellEllipsis>
                </div>
              ),
            },
            {
              column: (
                <div key="" className="clickable" onClick={() => navigate(`${row.id}`)}>
                  <CellDate value={row.createdOn} />
                </div>
              ),
            },
            {
              column: (
                <div key="" className="clickable" onClick={() => navigate(`${row.id}`)}>
                  <CellDate value={row.updatedOn} />
                </div>
              ),
            },
            {
              column: (
                <div key="" className="clickable" onClick={() => navigate(`${row.id}`)}>
                  <CellEllipsis key="">{row.status}</CellEllipsis>
                </div>
              ),
            },
          ];
        }}
      />
    </styled.WorkOrderList>
  );
};

export default WorkOrderList;
