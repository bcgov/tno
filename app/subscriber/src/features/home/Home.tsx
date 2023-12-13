import { MsearchMultisearchBody } from '@elastic/elasticsearch/lib/api/types';
import { DateFilter } from 'components/date-filter';
import { ContentActionBar } from 'components/tool-bar';
import { createFilterSettings } from 'features/utils';
import moment from 'moment';
import React, { useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import { Tooltip } from 'react-tooltip';
import { useContent } from 'store/hooks';
import {
  Checkbox,
  Col,
  FlexboxTable,
  generateQuery,
  IContentModel,
  IFilterSettingsModel,
  ITableInternalRow,
  Radio,
  Row,
  useWindowSize,
} from 'tno-core';

import { determineColumns } from './constants';
import * as styled from './styled';

/**
 * Home component that will be rendered when the user is logged in.
 */
export const Home: React.FC = () => {
  const [
    {
      home: { filter },
    },
    { findContentWithElasticsearch, storeHomeFilter: storeFilter },
  ] = useContent();
  const [selectAll, setSelectAll] = React.useState(false);
  const [homeItems, setHomeItems] = React.useState<IContentModel[]>([]);
  const [selected, setSelected] = React.useState<IContentModel[]>([]);
  const [disabledCols, setDisabledCols] = React.useState<string[]>([]);
  const [sortBy, setSortBy] = React.useState<'source' | 'time' | ''>('source');
  const navigate = useNavigate();
  const { width } = useWindowSize();
  const selectAllZone = document.querySelector('.table-container');
  const contentType = useMemo(() => {
    if (!!filter?.contentTypes?.length) return filter.contentTypes[0];
    else return 'all';
  }, [filter.contentTypes]);
  const [settings] = React.useState<IFilterSettingsModel>(
    createFilterSettings(
      filter.startDate ?? moment().startOf('day').toISOString(),
      filter.endDate ?? moment().endOf('day').toISOString(),
    ),
  );

  const fetchResults = React.useCallback(
    async (filter: MsearchMultisearchBody) => {
      try {
        const res: any = await findContentWithElasticsearch(filter, false);
        setHomeItems(res.hits.hits.map((h: { _source: IContentModel }) => h._source));
      } catch {}
    },
    [findContentWithElasticsearch],
  );

  /** when select all is toggled all items are selected */
  React.useEffect(() => {
    if (selectAll) setSelected(homeItems);
    if (!selectAll) setSelected([]);
  }, [homeItems, selectAll]);

  React.useEffect(() => {
    // stops invalid requests before filter is synced with date
    if (!filter.startDate) return;
    fetchResults(
      generateQuery({
        ...settings,
        contentTypes: !!contentType ? filter.contentTypes : [],
        startDate: filter.startDate,
        endDate: filter.endDate,
        mediaTypeIds: filter.mediaTypeIds ?? [],
        sourceIds: filter.sourceIds ?? [],
      }),
    );
  }, [fetchResults, filter, settings, contentType]);

  /** controls the checking and unchecking of rows in the list view */
  const handleSelectedRowsChanged = (row: ITableInternalRow<IContentModel>) => {
    if (row.isSelected) {
      setSelected(row.table.rows.filter((r) => r.isSelected).map((r) => r.original));
    } else {
      setSelected((selected) => selected.filter((r) => r.id !== row.original.id));
    }
  };

  return (
    <styled.Home>
      <Row>
        <ContentActionBar
          onList
          content={selected}
          setSelectAll={setSelectAll}
          selectAllZone={selectAllZone ?? undefined}
        />
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
      <DateFilter filter={filter} storeFilter={storeFilter} />
      <Row className="table-container">
        <FlexboxTable
          rowId="id"
          columns={determineColumns(contentType, width, disabledCols)}
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
