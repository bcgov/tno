import React from 'react';
import { useNavigate } from 'react-router-dom';
import { SortingRule } from 'react-table';
import { useWorkOrders } from 'store/hooks/admin/useWorkOrders';
import { Col, FlexboxTable, ITablePage, IWorkOrderModel, Page, Row } from 'tno-core';

import { getColumns } from './constants';
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

  const handleChangeSort = React.useCallback(
    (sortBy: SortingRule<IWorkOrderModel>[]) => {
      const sorts = sortBy.map((sb) => ({ id: sb.id, desc: sb.desc }));
      const same = sorts.every(
        (val, i) =>
          val.id === workOrderFilter.sort[i]?.id && val.desc === workOrderFilter.sort[i]?.desc,
      );
      if (!same) {
        storeFilter({ ...workOrderFilter, sort: sorts });
      }
    },
    [storeFilter, workOrderFilter],
  );

  const handleChangePage = React.useCallback(
    (page: ITablePage) => {
      if (
        workOrderFilter.pageIndex !== page.pageIndex ||
        workOrderFilter.pageSize !== page.pageSize
      ) {
        storeFilter({
          ...workOrderFilter,
          pageIndex: page.pageIndex,
          pageSize: page.pageSize ?? workOrderFilter.pageSize,
        });
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
    if (workOrderFilter !== filter) {
      setFilter(workOrderFilter);
      fetch(workOrderFilter);
    }
  }, [fetch, workOrderFilter, filter]);

  return (
    <styled.WorkOrderList>
      <Row className="add-media" justifyContent="flex-end">
        <Col flex="1 1 0">
          A work order is a request submitted to the system (i.e. transcription, NLP).
        </Col>
      </Row>
      <WorkOrderListFilter />
      <FlexboxTable
        rowId="id"
        columns={getColumns((contentId) => navigate(`/contents/${contentId}`))}
        data={page.items}
        manualPaging={true}
        pageIndex={workOrderFilter.pageIndex}
        pageSize={workOrderFilter.pageSize}
        pageCount={page.pageCount}
        showSort={true}
        onPageChange={handleChangePage}
        onSortChange={handleChangeSort}
        onRowClick={(row) => navigate(`${row.original.id}`)}
      />
    </styled.WorkOrderList>
  );
};

export default WorkOrderList;
