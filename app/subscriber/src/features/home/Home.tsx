import { DateFilter } from 'components/date-filter';
import {
  IContentListAdvancedFilter,
  IContentListFilter,
} from 'features/content/list-view/interfaces';
import { FolderMenu } from 'features/content/view-content/FolderMenu';
import React from 'react';
import { FaFolderPlus } from 'react-icons/fa';
import { useNavigate } from 'react-router-dom';
import { Tooltip } from 'react-tooltip';
import { useContent } from 'store/hooks';
import {
  ContentStatus,
  ContentTypeName,
  FlexboxTable,
  IContentModel,
  IFolderContentModel,
  ITableInternalRow,
  Page,
  Row,
  useWindowSize,
} from 'tno-core';

import { determineColumns } from './constants';
import { HomeFilters } from './home-filters';
import * as styled from './styled';
import { makeFilter } from './utils';

/**
 * Home component that will be rendered when the user is logged in.
 */
export const Home: React.FC = () => {
  const [{ filter, filterAdvanced }, { findContent }] = useContent();
  const [homeItems, setHomeItems] = React.useState<IContentModel[]>([]);
  const [selected, setSelected] = React.useState<IContentModel[]>([]);
  const navigate = useNavigate();
  const { width } = useWindowSize();

  const [, setLoading] = React.useState(false);
  const fetch = React.useCallback(
    async (filter: IContentListFilter & Partial<IContentListAdvancedFilter>) => {
      try {
        setLoading(true);
        const data = await findContent(
          makeFilter({
            ...filter,
            contentTypes:
              filter.contentTypes.length > 0 ? filter.contentTypes : [ContentTypeName.PrintContent],
            startDate: filter.startDate ? filter.startDate : new Date().toDateString(),
            sort: [{ id: 'source.sortOrder' }],
            status: ContentStatus.Published,
          }),
        );
        setHomeItems(data.items);
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

  /** controls the checking and unchecking of rows in the list view */
  const handleSelectedRowsChanged = (row: ITableInternalRow<IContentModel>) => {
    if (row.isSelected) {
      setSelected(row.table.rows.filter((r) => r.isSelected).map((r) => r.original));
    } else {
      setSelected((selected) => selected.filter((r) => r.id !== row.original.id));
    }
  };

  /** transform the content to folder content before sending it to the API */
  const toFolderContent = (content: IContentModel[]) => {
    return content.map((item) => {
      return {
        ...item,
        sortOrder: 0,
        contentId: item.id,
      } as IFolderContentModel;
    });
  };

  /** retrigger content fetch when change is applied */
  React.useEffect(() => {
    fetch({ ...filter, ...filterAdvanced });
  }, [filter, filterAdvanced, fetch]);

  return (
    <styled.Home>
      <Row>
        <div className="show-media-label">SHOW MEDIA TYPE:</div>
        <HomeFilters />
      </Row>
      <Row justifyContent="end">
        <FaFolderPlus className="add-folder" data-tooltip-id="folder" />
      </Row>
      <DateFilter />
      <Tooltip
        clickable
        variant="light"
        className="folder-menu"
        place="bottom"
        openOnClick
        style={{ opacity: '1', boxShadow: '0 0 8px #464545' }}
        id="folder"
      >
        <FolderMenu content={toFolderContent(selected)} />
      </Tooltip>
      <Row className="table-container">
        <FlexboxTable
          rowId="id"
          columns={determineColumns(filter.contentTypes[0], width)}
          isMulti
          groupBy={(item) => item.original.source?.name ?? ''}
          onRowClick={(e: any) => {
            navigate(`/view/${e.original.id}`);
          }}
          onSelectedChanged={handleSelectedRowsChanged}
          data={homeItems}
          pageButtons={5}
          showPaging={false}
        />
      </Row>
    </styled.Home>
  );
};
