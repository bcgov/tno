import React from 'react';
import { useNavigate, useParams } from 'react-router';
import { useContent } from 'store/hooks';
import {
  Col,
  ContentTypeName,
  FlexboxTable,
  IContentModel,
  ITableInternalRow,
  ITablePage,
  ITableSort,
  Page,
  replaceQueryParams,
  Row,
  Show,
  useCombinedView,
} from 'tno-core';

import { ContentForm } from '../form';
import { defaultPage } from '../list-view/constants';
import { IContentListAdvancedFilter } from '../list-view/interfaces';
import { ReportActions } from './components';
import { columns } from './constants';
import { IMorningReportsFilter } from './interfaces';
import { MorningReportsFilter } from './MorningReportsFilter';
import * as styled from './styled';
import { makeFilter } from './utils';

export interface IMorningReportsProps extends React.HTMLAttributes<HTMLDivElement> {}

/**
 * Provides a list view of print content to help select stories for the morning report.
 * @param props Component props.
 * @returns Component.
 */
export const MorningReports: React.FC<IMorningReportsProps> = (props) => {
  const { id: contentId = '' } = useParams();
  const navigate = useNavigate();
  const { combined } = useCombinedView();
  const [
    { filterMorningReports: filter, filterMorningReportAdvanced: filterAdvanced, content },
    { findContent, storeFilterMorningReport },
  ] = useContent();

  const [, setLoading] = React.useState(false);
  const [selected, setSelected] = React.useState<IContentModel[]>([]);

  const page = React.useMemo(
    () =>
      !!content
        ? new Page(content.page - 1, content.quantity, content?.items, content.total)
        : defaultPage,
    [content],
  );

  const fetch = React.useCallback(
    async (filter: IMorningReportsFilter & Partial<IContentListAdvancedFilter>) => {
      try {
        setLoading(true);
        const data = await findContent(
          makeFilter({
            ...filter,
          }),
        );
        return new Page(data.page - 1, data.quantity, data?.items, data.total);
      } catch (error) {
        // TODO: Handle error
        throw error;
      } finally {
        setLoading(false);
      }
    },
    [findContent],
  );

  React.useEffect(() => {
    fetch({ ...filter, ...filterAdvanced });
    // Do not want to fetch when the advanced filter changes.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [filter, fetch]);

  const handleRowClick = (row: ITableInternalRow<IContentModel>) => {
    navigate(`/morning/papers/combined/${row.original.id}${window.location.search}`);
  };

  const handleChangePage = React.useCallback(
    (page: ITablePage) => {
      if (filter.pageIndex !== page.pageIndex || filter.pageSize !== page.pageSize) {
        const newFilter = {
          ...filter,
          pageIndex: page.pageIndex,
          pageSize: page.pageSize ?? filter.pageSize,
        };
        storeFilterMorningReport(newFilter);
        replaceQueryParams(newFilter, { includeEmpty: false });
      }
    },
    [filter, storeFilterMorningReport],
  );

  const handleChangeSort = React.useCallback(
    (sort: ITableSort<IContentModel>[]) => {
      const sorts = sort.filter((s) => s.isSorted).map((s) => ({ id: s.id, desc: s.isSortedDesc }));
      storeFilterMorningReport({ ...filter, sort: sorts });
    },
    [filter, storeFilterMorningReport],
  );

  const handleSelectedRowsChanged = (row: ITableInternalRow<IContentModel>) => {
    if (row.isSelected) {
      setSelected(row.table.rows.filter((r) => r.isSelected).map((r) => r.original));
    } else {
      setSelected((selected) => selected.filter((r) => r.id !== row.original.id));
    }
  };

  return (
    <styled.MorningReports>
      <Col wrap="nowrap">
        <MorningReportsFilter onSearch={fetch} />
        <Row className="content-list">
          <FlexboxTable
            rowId="id"
            columns={columns}
            data={page.items}
            isMulti={true}
            manualPaging={true}
            pageIndex={filter.pageIndex}
            pageSize={filter.pageSize}
            pageCount={page.pageCount}
            showSort={true}
            activeRowId={contentId}
            onPageChange={handleChangePage}
            onSortChange={handleChangeSort}
            onRowClick={handleRowClick}
            onSelectedChanged={handleSelectedRowsChanged}
          />
        </Row>
        <ReportActions setLoading={setLoading} selected={selected} filter={filter} />
        <Show visible={combined}>
          <hr />
          <Row className="bottom-pane" id="bottom-pane">
            <ContentForm
              contentType={ContentTypeName.PrintContent}
              scrollToContent={false}
              combinedPath="/morning/papers/combined"
            />
          </Row>
        </Show>
      </Col>
    </styled.MorningReports>
  );
};
