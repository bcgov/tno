import { IWorkOrderModel } from 'hooks';
import React from 'react';
import { useNavigate } from 'react-router-dom';
import { SortingRule } from 'react-table';
import { useApp } from 'store/hooks';
import { useWorkOrders } from 'store/hooks/admin/useWorkOrders';
import { Col, Page, PagedTable, Row } from 'tno-core';

import { columns } from './constants';
import { IWorkOrderListFilter } from './interfaces/IWorkOrderListFilter';
import * as styled from './styled';
import { makeWorkOrderFilter } from './utils/makeWorkOrderFilter';
import { WorkOrderListFilter } from './WorkOrderListFilter';

/**
 * Provides a component for listing and filtering work orders.
 * @returns Component for listing and filtering work orders.
 */
export const WorkOrderList = () => {
  const navigate = useNavigate();
  const [{ workOrders, workOrderFilter }, { findWorkOrders, storeFilter }] = useWorkOrders();
  const [{ requests }] = useApp();
  const [page, setPage] = React.useState(
    new Page(workOrders.page - 1, workOrders.quantity, workOrders.items, workOrders.total),
  );

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
    (pi: number, ps?: number) => {
      if (workOrderFilter.pageIndex !== pi || workOrderFilter.pageSize !== ps) {
        storeFilter({
          ...workOrderFilter,
          pageIndex: pi,
          pageSize: ps ?? workOrderFilter.pageSize,
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
    fetch(workOrderFilter);
  }, [fetch, workOrderFilter]);

  return (
    <styled.WorkOrderList>
      <Row className="add-media" justifyContent="flex-end">
        <Col flex="1 1 0">
          A work order is a request submitted to the system (i.e. transcription, NLP).
        </Col>
      </Row>
      <PagedTable
        columns={columns}
        header={WorkOrderListFilter}
        sorting={{ sortBy: workOrderFilter.sort }}
        isLoading={!!requests.length}
        page={page}
        onRowClick={(row) => navigate(`${row.original.id}`)}
        onChangeSort={handleChangeSort}
        onChangePage={handleChangePage}
      ></PagedTable>
    </styled.WorkOrderList>
  );
};
