import React from 'react';
import { useNavigate, useParams } from 'react-router';
import { SortingRule } from 'react-table';
import { Row as TRow } from 'react-table';
import { useContent } from 'store/hooks';
import {
  Col,
  ContentTypeName,
  IContentModel,
  Page,
  PagedTable,
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
    { filterMorningReports: filter, filterAdvanced, content },
    { findContent, storeMorningReportFilter },
  ] = useContent();

  const [loading, setLoading] = React.useState(false);
  const [selected, setSelected] = React.useState<IContentModel[]>([]);

  const selectedRowIds = !!contentId
    ? ({ [contentId]: true } as Record<string, boolean>)
    : undefined;

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

  const handleRowClick = (row: TRow<IContentModel>) => {
    if (row.isSelected)
      navigate(`/morning/papers/combined/${row.original.id}${window.location.search}`);
  };

  const handleChangePage = React.useCallback(
    (pi: number, ps: number) => {
      if (filter.pageIndex !== pi || filter.pageSize !== ps) {
        const newFilter = { ...filter, pageIndex: pi, pageSize: ps ?? filter.pageSize };
        storeMorningReportFilter(newFilter);
        replaceQueryParams(newFilter, { includeEmpty: false });
      }
    },
    [filter, storeMorningReportFilter],
  );

  const handleChangeSort = React.useCallback(
    (sortBy: SortingRule<IContentModel>[]) => {
      const sorts = sortBy.map((sb) => ({ id: sb.id, desc: sb.desc }));
      storeMorningReportFilter({ ...filter, sort: sorts });
    },
    [filter, storeMorningReportFilter],
  );

  const handleSelectedRowsChanged = (selectedRowIds: TRow<IContentModel>[]) => {
    if (
      !loading &&
      (selectedRowIds.length !== selected.length ||
        !selectedRowIds.every((r) => selected.some((s) => s.id === r.original.id)))
    ) {
      setSelected(selectedRowIds.map((r) => r.original));
    }
  };

  const handleActiveRowChange = (row?: TRow<IContentModel>) => {
    if (row) handleRowClick(row);
  };

  return (
    <styled.MorningReports>
      <Col wrap="nowrap">
        <MorningReportsFilter onSearch={fetch} />
        <Row className="content-list">
          <PagedTable
            columns={columns}
            isMulti
            autoResetSelectedRows={false}
            maintainSelectedRows={true}
            page={page}
            paging={{ pageSize: filter.pageSize }}
            isLoading={loading}
            sorting={{ sortBy: filter.sort }}
            getRowId={(content) => content.id.toString()}
            selectedRowIds={selectedRowIds}
            onRowClick={handleRowClick}
            onSelectedRowsChange={handleSelectedRowsChanged}
            onActiveRowChange={handleActiveRowChange}
            onChangePage={handleChangePage}
            onChangeSort={handleChangeSort}
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
