import { DateFilter } from 'components/date-filter';
import { FolderSubMenu } from 'components/folder-sub-menu';
import {
  IContentListAdvancedFilter,
  IContentListFilter,
} from 'features/content/list-view/interfaces';
import React from 'react';
import { FaEllipsisVertical } from 'react-icons/fa6';
import { useNavigate } from 'react-router-dom';
import { Tooltip } from 'react-tooltip';
import { useContent } from 'store/hooks';
import {
  Checkbox,
  Col,
  ContentStatus,
  ContentTypeName,
  FlexboxTable,
  IContentModel,
  ITableInternalRow,
  Page,
  Radio,
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
  const [disabledCols, setDisabledCols] = React.useState<string[]>([]);
  const [sortBy, setSortBy] = React.useState<'source' | 'time' | ''>('source');
  const navigate = useNavigate();
  const { width } = useWindowSize();

  const [, setLoading] = React.useState(false);
  const fetch = React.useCallback(
    async (filter: IContentListFilter & Partial<IContentListAdvancedFilter>) => {
      try {
        setLoading(true);
        const filters = makeFilter({
          ...filter,
          contentTypes:
            filter.contentTypes.length > 0 ? filter.contentTypes : [ContentTypeName.PrintContent],
          startDate: filter.startDate ? filter.startDate : new Date().toDateString(),
          status: ContentStatus.Published,
        });
        const data = await findContent({
          ...filters,
          sort: sortBy === 'time' ? ['publishedOn'] : ['source.sortOrder'],
        });
        setHomeItems(data.items);
        return new Page(data.page - 1, data.quantity, data?.items, data.total);
      } catch (error) {
        // TODO: Handle error
        throw error;
      } finally {
        setLoading(false);
      }
    },
    [findContent, sortBy],
  );

  /** controls the checking and unchecking of rows in the list view */
  const handleSelectedRowsChanged = (row: ITableInternalRow<IContentModel>) => {
    if (row.isSelected) {
      setSelected(row.table.rows.filter((r) => r.isSelected).map((r) => r.original));
    } else {
      setSelected((selected) => selected.filter((r) => r.id !== row.original.id));
    }
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
        <FaEllipsisVertical data-tooltip-id="view-options" className="more-options" />
        <FolderSubMenu selectedContent={selected} />

        <Tooltip place="right" className="view-options" openOnClick id="view-options" clickable>
          <Col>
            <div className="show-section">
              <b>SHOW:</b>
              <Checkbox disabled className="option" label={'TEASERS'} />
              <Checkbox
                label={'SENTIMENT'}
                className="option"
                defaultChecked={!disabledCols.includes('tone')}
                onClick={(e) => {
                  if (!(e.target as HTMLInputElement).checked)
                    setDisabledCols((disabledCols) => [...disabledCols, 'tone']);
                  else
                    setDisabledCols((disabledCols) => disabledCols.filter((col) => col !== 'tone'));
                }}
              />
              <Checkbox
                label={'PAGE NUMBERS'}
                className="option"
                defaultChecked={!disabledCols.includes('sectionPage')}
                onClick={(e) => {
                  if (!(e.target as HTMLInputElement).checked)
                    setDisabledCols((disabledCols) => [...disabledCols, 'sectionPage']);
                  else
                    setDisabledCols((disabledCols) =>
                      disabledCols.filter((col) => col !== 'sectionPage'),
                    );
                }}
              />
            </div>
            <Col className="sort-section">
              <b>GROUP BY:</b>
              <Row className="option">
                <Radio
                  checked={sortBy === 'source'}
                  onClick={(e) =>
                    (e.target as HTMLInputElement).checked ? setSortBy('source') : setSortBy('')
                  }
                />
                <label>MEDIA SOURCE</label>
              </Row>
              <Row className="option">
                <Radio
                  checked={sortBy === 'time'}
                  disabled
                  className="option"
                  onClick={(e) =>
                    (e.target as HTMLInputElement).checked ? setSortBy('time') : setSortBy('')
                  }
                />
                <label>TIME</label>
              </Row>
            </Col>
          </Col>
        </Tooltip>
      </Row>
      <DateFilter />
      <Row className="table-container">
        <FlexboxTable
          rowId="id"
          columns={determineColumns(filter.contentTypes[0], width, disabledCols)}
          isMulti
          groupBy={(item) => {
            if (item.original.source?.name && sortBy === 'source')
              return item.original.source?.name;
            else if (item.original.publishedOn && sortBy === 'time')
              return item.original.publishedOn;
            else return ' ';
          }}
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
