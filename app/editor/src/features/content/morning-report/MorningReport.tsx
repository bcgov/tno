import { ContentTypeName, IContentModel, useCombinedView } from 'hooks';
import React from 'react';
import { useNavigate, useParams } from 'react-router';
import { SortingRule } from 'react-table';
import { Row as TRow } from 'react-table';
import { useContent } from 'store/hooks';
import { Col, Page, PagedTable, Row, Show } from 'tno-core';

import { ContentForm } from '../form';
import { defaultPage } from '../list-view/constants';
import { IContentListAdvancedFilter } from '../list-view/interfaces';
import { columns } from './constants';
import { IMorningReportFilter } from './interfaces';
import { MorningReportFilter } from './MorningReportFilter';
import { ReportActions } from './ReportActions';
import * as styled from './styled';
import { makeFilter } from './utils';

export interface IMorningReportProps extends React.HTMLAttributes<HTMLDivElement> {}

export const MorningReport: React.FC<IMorningReportProps> = (props) => {
  const { id: contentId = '' } = useParams();
  const navigate = useNavigate();
  const [
    { morningReportFilter: filter, filterAdvanced, content },
    { findContent, storeMorningReportFilter },
  ] = useContent();

  const [loading, setLoading] = React.useState(false);
  const [selected, setSelected] = React.useState<IContentModel[]>([]);
  const { combined } = useCombinedView();
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
    async (filter: IMorningReportFilter & Partial<IContentListAdvancedFilter>) => {
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
    navigate(`/morning/reports/combined/${row.original.id}`);
  };

  const handleChangePage = React.useCallback(
    (pi: number, ps: number) => {
      if (filter.pageIndex !== pi || filter.pageSize !== ps) {
        storeMorningReportFilter({ ...filter, pageIndex: pi, pageSize: ps ?? filter.pageSize });
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

  return (
    <styled.MorningReport maxWidth={''}>
      <Col wrap="nowrap">
        <MorningReportFilter onSearch={fetch} />
        <Row className="content-list">
          <PagedTable
            columns={columns}
            isMulti
            autoResetSelectedRows={false}
            maintainSelectedRows={true}
            page={page}
            isLoading={loading}
            sorting={{ sortBy: filter.sort }}
            getRowId={(content) => content.id.toString()}
            selectedRowIds={selectedRowIds}
            onRowClick={handleRowClick}
            onSelectedRowsChange={handleSelectedRowsChanged}
            onChangePage={handleChangePage}
            onChangeSort={handleChangeSort}
          />
        </Row>
        <ReportActions setLoading={setLoading} selected={selected} />
        <Show visible={combined}>
          <hr />
          <Row className="bottom-pane" id="bottom-pane">
            <ContentForm contentType={ContentTypeName.PrintContent} />
          </Row>
        </Show>
      </Col>
    </styled.MorningReport>
  );
};
