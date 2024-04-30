import { Status } from 'components/status';
import { TabControl } from 'components/tab-control';
import { AdvancedSearchKeys } from 'features/content/constants';
import { IContentListAdvancedFilter, IContentListFilter } from 'features/content/interfaces';
import { naturalSortValue } from 'features/content/list-view/utils/naturalSort';
import { useContent } from 'store/hooks';
import { IContentSearchResult } from 'store/slices';
import { CellEllipsis, Checkbox, ITableHookColumn, LogicalOperator, Page, Row } from 'tno-core';

export interface IColumnProps {
  fetch: (
    filter: IContentListFilter & Partial<IContentListAdvancedFilter>,
  ) => Promise<Page<IContentSearchResult> | undefined>;
  onClickUse: (content: IContentSearchResult) => void;
}

export const useColumns = ({
  fetch,
  onClickUse,
}: IColumnProps): ITableHookColumn<IContentSearchResult>[] => {
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
          onClick={(status) => onClickUse?.({ ...cell.original, status: status })}
        />
      ),
    },
  ];
};
