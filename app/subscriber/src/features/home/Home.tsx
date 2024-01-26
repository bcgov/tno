import { MsearchMultisearchBody } from '@elastic/elasticsearch/lib/api/types';
import { DateFilter } from 'components/date-filter';
import { ContentListActionBar } from 'components/tool-bar';
import { filterFormat } from 'features/search-page/utils';
import { createFilterSettings } from 'features/utils';
import { IContentSearchResult } from 'features/utils/interfaces';
import moment from 'moment';
import React, { useMemo } from 'react';
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
import * as styled from './styled';

/**
 * Home component that will be rendered when the user is logged in.
 */
export const Home: React.FC = () => {
  const [
    {
      home: { filter },
    },
    { findContentWithElasticsearch, storeHomeFilter: storeFilter, stream },
  ] = useContent();
  const navigate = useNavigate();
  const { width } = useWindowSize();

  const [content, setContent] = React.useState<IContentSearchResult[]>([]);
  const [selected, setSelected] = React.useState<IContentModel[]>([]);
  const [disabledCols, setDisabledCols] = React.useState<string[]>([]);
  const [sortBy, setSortBy] = React.useState<'source' | 'time' | ''>('source');
  const [settings] = React.useState<IFilterSettingsModel>(
    createFilterSettings(
      filter.startDate ?? moment().startOf('day').toISOString(),
      filter.endDate ?? moment().endOf('day').toISOString(),
    ),
  );

  const selectedIds = selected.map((i) => i.id.toString());

  const [{ actions }] = useLookup();
  const contentType = useMemo(() => {
    if (!!filter?.contentTypes?.length) return filter.contentTypes[0];
    else return 'all';
  }, [filter.contentTypes]);

  const createStream = async (item: IContentSearchResult) => {
    const fileReference = item?.fileReferences ? item?.fileReferences[0] : undefined;
    if (!!fileReference) return stream(fileReference.path);
    return undefined;
  };

  const sortFunc = (key: string) => {
    switch (key) {
      case 'published':
        return (a: IContentModel, b: IContentModel) => (a.publishedOn > b.publishedOn ? 1 : -1);
      case 'source':
        return (a: IContentModel, b: IContentModel) => {
          if (a.source && b.source) {
            return a.source.sortOrder > b.source.sortOrder ? 1 : -1;
          }
          return -1;
        };
      default:
        return (a: IContentModel, b: IContentModel) => (a.publishedOn > b.publishedOn ? 1 : -1);
    }
  };

  const fetchResults = React.useCallback(
    async (filter: MsearchMultisearchBody) => {
      try {
        let firstSort = '';
        let secondSort = '';
        switch (sortBy) {
          case 'time':
            firstSort = 'source';
            secondSort = 'published';
            break;
          case 'source':
            firstSort = 'published';
            secondSort = 'source';
            break;
          default:
            firstSort = 'published';
            secondSort = 'source';
        }
        const res: any = await findContentWithElasticsearch(filter, false);
        setContent(
          res.hits.hits
            .map((h: { _source: IContentModel }) => h._source)
            .sort(sortFunc(firstSort))
            .sort(sortFunc(secondSort)),
        );
      } catch {}
    },
    [findContentWithElasticsearch, sortBy],
  );

  const displayMedia = async (r: IContentSearchResult) => {
    const list = [...content];
    const e = list.find((e) => e.id === r.id);
    if (!!e) {
      if (!e.mediaUrl) {
        createStream(e).then((result) => {
          e.mediaUrl = result;
          e.displayMedia = result !== undefined ? true : false;
        });
      } else {
        e.displayMedia = !e.displayMedia;
      }
      setContent(list);
    }
  };

  React.useEffect(() => {
    // stops invalid requests before filter is synced with date
    if (!filter.startDate) return;
    fetchResults(
      generateQuery(
        filterFormat({
          ...settings,
          contentTypes: !!contentType ? filter.contentTypes : [],
          featured: true,
          startDate: filter.startDate,
          endDate: filter.endDate,
          mediaTypeIds: filter.mediaTypeIds ?? [],
          sourceIds: filter.sourceIds ?? [],
        }),
      ),
    );
  }, [fetchResults, filter, settings, contentType, actions]);

  /** controls the checking and unchecking of rows in the list view */
  const handleSelectedRowsChanged = React.useCallback(
    (row: ITableInternalRow<IContentSearchResult>) => {
      if (row.isSelected) {
        setSelected(row.table.rows.filter((r) => r.isSelected).map((r) => r.original));
      } else {
        setSelected((selected) => selected.filter((r) => r.id !== row.original.id));
      }
    },
    [],
  );

  return (
    <styled.Home>
      <Row>
        <ContentListActionBar
          content={selected}
          onSelectAll={(e) => (e.target.checked ? setSelected(content) : setSelected([]))}
        />
        <Tooltip place="right" className="view-options" openOnClick id="view-options" clickable>
          <Col>
            <div className="show-section">
              <b>SHOW:</b>
              <Checkbox
                label={'TEASERS'}
                className="option"
                defaultChecked={!disabledCols.includes('teaser')}
                onClick={(e) => {
                  if (!(e.target as HTMLInputElement).checked)
                    setDisabledCols((disabledCols) => [...disabledCols, 'teaser']);
                  else
                    setDisabledCols((disabledCols) =>
                      disabledCols.filter((col) => col !== 'teaser'),
                    );
                }}
              />
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
                  onChange={(e) =>
                    (e.target as HTMLInputElement).checked ? setSortBy('source') : setSortBy('')
                  }
                />
                <label>MEDIA SOURCE</label>
              </Row>
              <Row className="option">
                <Radio
                  checked={sortBy === 'time'}
                  className="option"
                  onChange={(e) =>
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
          columns={determineColumns(contentType, width, disabledCols, displayMedia)}
          isMulti
          groupBy={(item) => {
            if (item.original.source?.name && sortBy === 'source')
              return item.original.source?.name;
            else if (item.original.publishedOn && sortBy === 'time')
              return new Date(item.original.publishedOn).toLocaleDateString('en-US', {
                year: 'numeric',
                month: 'numeric',
                day: 'numeric',
                hour: '2-digit',
                minute: '2-digit',
                hour12: false,
              });
            else return ' ';
          }}
          onRowClick={(e: any) => {
            navigate(`/view/${e.original.id}`);
          }}
          onSelectedChanged={handleSelectedRowsChanged}
          selectedRowIds={selectedIds}
          data={content}
          pageButtons={5}
          showPaging={false}
          showHeader={false}
        />
      </Row>
    </styled.Home>
  );
};
