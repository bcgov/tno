import { Status } from '@/editor/components/status';
import { TabControl } from '@/editor/components/tab-control';
import { AdvancedSearchKeys } from '@/editor/features/content/constants';
import {
  type IContentListAdvancedFilter,
  type IContentListFilter,
} from '@/editor/features/content/interfaces';
import { naturalSortValue } from '@/editor/features/content/list-view/utils/naturalSort';
import { useContent } from '@/editor/store/hooks';
import { type IContentSearchResult } from '@/editor/store/slices';
import {
  CellEllipsis,
  Checkbox,
  type ITableHookColumn,
  LogicalOperator,
  type Page,
  Row,
} from 'tno-core';

export interface IColumnProps {
  fetch: (
    filter: IContentListFilter & Partial<IContentListAdvancedFilter>,
  ) => Promise<Page<IContentSearchResult> | undefined>;
  onClickUse: (content: IContentSearchResult) => void;
}

export const useColumns = ({
  fetch,
  onClickUse,
}: IColumnProps): Array<ITableHookColumn<IContentSearchResult>> => {
  const [{ filterPaper, filterPaperAdvanced }, { storeFilterPaperAdvanced }] = useContent();

  return [
    {
      accessor: 'headline',
      label: (
        <Row gap="0.5rem">
          <TabControl />
          Headline
        </Row>
      ),
      showSort: false,
      cell: (cell) => <CellEllipsis>{cell.original.headline}</CellEllipsis>,
      width: 6,
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
      width: 2,
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
              filterPaperAdvanced.fieldType === AdvancedSearchKeys.Page &&
              filterPaperAdvanced.searchTerm === '?*'
            }
            onChange={async (e) => {
              const values = {
                ...filterPaperAdvanced,
                fieldType: AdvancedSearchKeys.Page,
                searchTerm: e.target.checked ? '?*' : '',
                logicalOperator: LogicalOperator.Equals,
              };
              storeFilterPaperAdvanced(values);
              await fetch({ ...filterPaper, ...values });
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
      accessor: 'status',
      label: 'Use',
      hAlign: 'center',
      width: '55px',
      cell: (cell) => (
        <Status
          value={cell.original.status}
          onClick={(status) => {
            onClickUse?.({ ...cell.original, status });
          }}
        />
      ),
    },
  ];
};
