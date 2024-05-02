import { Status } from 'components/status';
import { TabControl } from 'components/tab-control';
import { AdvancedSearchKeys } from 'features/content/constants';
import { IContentListAdvancedFilter, IContentListFilter } from 'features/content/interfaces';
import { FaBug, FaCheckCircle, FaClock, FaFeather } from 'react-icons/fa';
import { FaCirclePause, FaRegCircleRight } from 'react-icons/fa6';
import { useContent } from 'store/hooks';
import { IContentSearchResult } from 'store/slices';
import {
  CellDate,
  CellEllipsis,
  Checkbox,
  formatIdirUsername,
  ITableHookColumn,
  LogicalOperator,
  Page,
  Row,
  Show,
  Spinner,
  WorkOrderStatusName,
} from 'tno-core';

import { naturalSortValue } from '../utils/naturalSort';

export interface IColumnProps {
  fetch: (
    filter: IContentListFilter & Partial<IContentListAdvancedFilter>,
  ) => Promise<Page<IContentSearchResult> | undefined>;
}

export const useColumns = ({ fetch }: IColumnProps): ITableHookColumn<IContentSearchResult>[] => {
  const [{ filter, filterAdvanced }, { storeFilterAdvanced }] = useContent();

  let columns: ITableHookColumn<IContentSearchResult>[] = [
    {
      accessor: 'headline',
      label: (
        <Row gap="0.5rem">
          <TabControl />
          Headline
        </Row>
      ),
      cell: (cell) => (
        <Row nowrap gap="0.5rem">
          <Show visible={cell.row.original.hasTranscript}>
            <FaFeather
              title={cell.row.original.isApproved ? 'Transcription' : 'Ready to review'}
              className={cell.row.original.isApproved ? 'completed' : 'ready'}
            />
          </Show>
          <CellEllipsis>
            <span>{cell.original.headline}</span>
          </CellEllipsis>
        </Row>
      ),
      width: 5,
      showSort: false,
    },
    {
      accessor: 'otherSource',
      label: 'Source',
      cell: (cell) => <CellEllipsis>{cell.original.otherSource}</CellEllipsis>,
    },
    {
      accessor: 'mediaType',
      label: 'Media Type',
      cell: (cell) => <CellEllipsis>{cell.original.mediaType}</CellEllipsis>,
      width: 1,
    },
    {
      accessor: 'section',
      sort: (row) => naturalSortValue(row.original),
      label: (
        <Row nowrap>
          Page:Section
          <Checkbox
            name="page"
            checked={
              filterAdvanced.fieldType === AdvancedSearchKeys.Page &&
              filterAdvanced.searchTerm === '?*'
            }
            onChange={async (e) => {
              const values = {
                ...filterAdvanced,
                fieldType: AdvancedSearchKeys.Page,
                searchTerm: e.target.checked ? '?*' : '',
                logicalOperator: LogicalOperator.Equals,
              };
              storeFilterAdvanced(values);
              await fetch({ ...filter, ...values });
            }}
          />
        </Row>
      ),
      cell: (cell) => {
        const separator = cell.original.page && cell.original.section ? ':' : '';
        const value = `${cell.original.page}${separator}${cell.original.section}`;
        return <CellEllipsis>{value}</CellEllipsis>;
      },
      width: 2,
    },
    {
      accessor: 'owner',
      label: 'User',
      cell: (cell) => <CellEllipsis>{formatIdirUsername(cell.original.owner)}</CellEllipsis>,
      width: 1,
    },
    {
      accessor: 'publishedOn',
      label: 'Pub Date',
      cell: (cell) => <CellDate value={cell.original.publishedOn} />,
      width: '180px',
      hAlign: 'center',
    },
    {
      accessor: 'status',
      label: 'Use',
      hAlign: 'center',
      width: '55px',
      cell: (cell) => <Status value={cell.original.status} />,
    },
  ];

  if (filter.pendingTranscript) {
    columns.push({
      accessor: 'transcript',
      label: 'Transcript',
      hAlign: 'center',
      width: '155px',
      cell: (cell) => {
        if (cell.original.transcriptStatus === WorkOrderStatusName.InProgress)
          return <Spinner size="8px" title="Transcribing" />;
        if (
          cell.original.transcriptStatus === WorkOrderStatusName.Completed &&
          !cell.original.isApproved
        )
          return <FaRegCircleRight className="completed" title="Ready to review" />;
        if (
          cell.original.transcriptStatus === WorkOrderStatusName.Completed &&
          cell.original.isApproved
        )
          return <FaCheckCircle className="completed" title="Completed" />;
        if (cell.original.transcriptStatus === WorkOrderStatusName.Submitted)
          return <FaClock className="submitted" title="Submitted" />;
        if (cell.original.transcriptStatus === WorkOrderStatusName.Failed)
          return <FaBug className="failed" title="Failed" />;
        if (cell.original.transcriptStatus === WorkOrderStatusName.Cancelled)
          return <FaCirclePause className="cancelled" title="Cancelled" />;
        return cell.original.transcriptStatus;
      },
    });
  }

  return columns;
};
