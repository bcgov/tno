// eslint-disable-next-line @typescript-eslint/no-unused-vars
import { AxiosError } from 'axios';
import {
  ContentListActionName,
  IContentListModel,
  IContentModel,
  useApiMorningReports,
} from 'hooks';
import React from 'react';
import { useParams } from 'react-router';
import { SortingRule } from 'react-table';
import { Row as TRow } from 'react-table';
import { toast } from 'react-toastify';
import { useContent } from 'store/hooks';
import { useContentStore } from 'store/slices';
import { Button, ButtonVariant, Col, FieldSize, Page, PagedTable, Row, Text } from 'tno-core';

import { defaultPage } from '../list-view/constants';
import { columns } from './constants';
import { IMorningReportFilter } from './interfaces';
import { MorningReportFilter } from './MorningReportFilter';
import * as styled from './styled';
import { makeFilter } from './utils';

export interface IMorningReportProps extends React.HTMLAttributes<HTMLDivElement> {}

export const MorningReport: React.FC<IMorningReportProps> = (props) => {
  const { id: contentId = '' } = useParams();
  const [{ morningReportFilter: filter, content }, { findContent, storeMorningReportFilter }] =
    useContent();
  const [, { updateContent, removeContent }] = useContentStore();
  const morningReports = useApiMorningReports();

  const [loading, setLoading] = React.useState(false);
  const [selected, setSelected] = React.useState<IContentModel[]>([]);
  const [commentary, setCommentary] = React.useState('1');

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
    async (filter: IMorningReportFilter) => {
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
    fetch(filter);
  }, [fetch, filter]);

  const handleRowClick = (row: TRow<IContentModel>) => {
    // TODO: Open up ContentForm
    // setContentType(content.contentType);
    // navigate(`/morning/reports/${content.id}`);
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

  const handleAction = async (action: ContentListActionName, name?: string, value?: string) => {
    try {
      setLoading(true);
      const model: IContentListModel = {
        action,
        actionName: name,
        actionValue: value,
        contentIds: selected.map((s) => s.id),
      };
      const res = await morningReports.updateContent(model);
      switch (action) {
        case ContentListActionName.Publish:
        case ContentListActionName.Unpublish:
        case ContentListActionName.Action:
          updateContent(
            res.data.map((i) => {
              i.isSelected = true;
              return i;
            }),
          );
          break;
        case ContentListActionName.Remove:
          removeContent(res.data);
          break;
      }
    } catch (ex: any | AxiosError) {
      toast.error(ex.message);
    } finally {
      setLoading(false);
    }
  };

  return (
    <styled.MorningReport>
      <Col>
        <MorningReportFilter />
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
        <Row>
          <Button
            variant={ButtonVariant.warning}
            disabled={!selected.length}
            onClick={() => handleAction(ContentListActionName.Remove)}
          >
            Remove
          </Button>
          <Button
            variant={ButtonVariant.primary}
            disabled={!selected.length}
            onClick={() => handleAction(ContentListActionName.Action, 'Front Page', 'true')}
          >
            Front Page
          </Button>
          <Button
            variant={ButtonVariant.primary}
            disabled={!selected.length}
            onClick={() => handleAction(ContentListActionName.Action, 'Top Story', 'true')}
          >
            Top Story
          </Button>
          <Text
            name="commentary"
            width={FieldSize.Tiny}
            type="number"
            value={commentary}
            onChange={(e) => setCommentary(e.currentTarget.value)}
          >
            <Button
              variant={ButtonVariant.primary}
              disabled={!selected.length}
              onClick={() => handleAction(ContentListActionName.Action, 'Commentary', commentary)}
            >
              Commentary
            </Button>
          </Text>
          <Button
            variant={ButtonVariant.success}
            disabled={!selected.length}
            onClick={() => handleAction(ContentListActionName.Publish)}
          >
            Publish
          </Button>
          <Button
            variant={ButtonVariant.secondary}
            disabled={!selected.length}
            onClick={() => handleAction(ContentListActionName.Unpublish)}
          >
            Unpublish
          </Button>
        </Row>
      </Col>
    </styled.MorningReport>
  );
};
