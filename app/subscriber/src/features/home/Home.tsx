import { MsearchMultisearchBody } from '@elastic/elasticsearch/lib/api/types';
import { DateFilter } from 'components/date-filter';
import { FolderSubMenu } from 'components/folder-sub-menu';
import { createFilterSettings } from 'features/press-gallery/utils';
import moment from 'moment';
import React from 'react';
import { FaEllipsisVertical } from 'react-icons/fa6';
import { useNavigate } from 'react-router-dom';
import { Tooltip } from 'react-tooltip';
import { useContent, useLookup } from 'store/hooks';
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
import { HomeFilters } from './home-filters';
import * as styled from './styled';

/**
 * Home component that will be rendered when the user is logged in.
 */
export const Home: React.FC = () => {
  const [{ filter, filterAdvanced }, { findContent, findContentWithElasticsearch }] = useContent();
  const [homeItems, setHomeItems] = React.useState<IContentModel[]>([]);
  const [selected, setSelected] = React.useState<IContentModel[]>([]);
  const [disabledCols, setDisabledCols] = React.useState<string[]>([]);
  const [sortBy, setSortBy] = React.useState<'source' | 'time' | ''>('source');
  const navigate = useNavigate();
  const { width } = useWindowSize();
  const [settings] = React.useState<IFilterSettingsModel>(
    createFilterSettings(
      filterAdvanced.startDate ?? moment().startOf('day').toISOString(),
      filterAdvanced.endDate ?? moment().endOf('day').toISOString(),
    ),
  );

  const [, setLoading] = React.useState(false);

  const fetchResults = React.useCallback(
    async (filter: MsearchMultisearchBody) => {
      try {
        const res: any = await findContentWithElasticsearch(filter, false);
        setHomeItems(res.hits.hits.map((h: { _source: IContentModel }) => h._source));
      } catch {}
    },
    [findContentWithElasticsearch],
  );

  React.useEffect(() => {
    // only fetch once the aliases are ready
    fetchResults(
      generateQuery({
        ...settings,
        contentTypes: filter.contentTypes.length > 0 ? filter.contentTypes : [],
        startDate: filterAdvanced.startDate ? filterAdvanced.startDate : new Date().toDateString(),
        endDate: filterAdvanced.endDate ? filterAdvanced.endDate : new Date().toDateString(),
        productIds: filter.productIds ?? [],
        sourceIds: filter.sourceIds ?? [],
      }),
    );
  }, [fetchResults, filterAdvanced, filter, settings]);

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
