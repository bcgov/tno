import { TabControl } from 'components/tab-control';
import { AdvancedSearchKeys } from 'features/content/list-view/constants';
import { IContentListAdvancedFilter } from 'features/content/list-view/interfaces';
import { getStatusText } from 'features/content/list-view/utils';
import { useContent } from 'store/hooks';
import { IContentSearchResult } from 'store/slices';
import {
  CellEllipsis,
  Checkbox,
  IContentModel,
  ITableHookColumn,
  LogicalOperator,
  Page,
  Row,
} from 'tno-core';

import { PublishedStatuses } from '../constants';
import { IPaperFilter } from '../interfaces';
import { changeStatus } from '../utils';

export interface IColumnProps {
  fetch: (
    filter: IPaperFilter & Partial<IContentListAdvancedFilter>,
  ) => Promise<Page<IContentModel>>;
  onClickUse: (content: IContentSearchResult) => void;
}

export const useColumns = ({
  fetch,
  onClickUse,
}: IColumnProps): ITableHookColumn<IContentSearchResult>[] => {
  const [{ filterPaper, filterAdvanced }, { storeFilterAdvanced }] = useContent();

  return [
    {
      accessor: 'headline',
      label: (
        <Row gap="0.5rem">
          <TabControl />
          Headline
        </Row>
      ),
      cell: (cell) => <CellEllipsis>{cell.original.headline}</CellEllipsis>,
      width: 6,
    },
    {
      accessor: 'otherSource',
      label: 'Source',
      cell: (cell) => <CellEllipsis>{cell.original.otherSource}</CellEllipsis>,
    },
    {
      accessor: 'product',
      label: 'Product',
      cell: (cell) => <CellEllipsis>{cell.original.product}</CellEllipsis>,
      width: 2,
    },
    {
      accessor: 'section',
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
      label: 'Status',
      hAlign: 'center',
      cell: (cell) => getStatusText(cell.original.status),
    },
    {
      accessor: 'status',
      label: 'Use',
      cell: (cell) => (
        <div className="center">
          <Checkbox
            name="publish"
            id={`publish-${cell.original.id}`}
            value={true}
            checked={PublishedStatuses.includes(cell.original.status)}
            onChange={() =>
              onClickUse?.({ ...cell.original, status: changeStatus(cell.original.status) })
            }
          />
        </div>
      ),
      hAlign: 'center',
    },
  ];
};
