import { MsearchMultisearchBody } from '@elastic/elasticsearch/lib/api/types';
import { ContentList } from 'components/content-list';
import { DateFilter } from 'components/date-filter';
import { ContentListActionBar } from 'components/tool-bar';
import { filterFormat } from 'features/search-page/utils';
import { createFilterSettings, getBooleanActionValue } from 'features/utils';
import { IContentSearchResult } from 'features/utils/interfaces';
import moment from 'moment';
import React from 'react';
import { useApp, useContent, useSettings } from 'store/hooks';
import { generateQuery, IContentModel, Loading, Row, Show } from 'tno-core';

import * as styled from './styled';

/**
 * Home component that will be rendered when the user is logged in.
 */
export const Home: React.FC = () => {
  window.snowplow('trackPageView');
  const [
    {
      home: { filter },
    },
    { findContentWithElasticsearch, storeHomeFilter: storeFilter },
  ] = useContent();
  const [{ userInfo }] = useApp();

  const [content, setContent] = React.useState<IContentSearchResult[]>([]);
  const { featuredStoryActionId } = useSettings(true);
  const [stateByDate, setStateByDate] = React.useState<{
    [date: string]: { selected: IContentModel[]; isSelectAllChecked: boolean };
  }>({});
  const [loading, setLoading] = React.useState(false);
  const handleContentSelected = React.useCallback(
    (selectedContent: IContentModel[]) => {
      const dateKey = filter.startDate || moment().startOf('day').toISOString();
      setStateByDate((prevState) => ({
        ...prevState,
        [dateKey]: {
          ...prevState[dateKey],
          selected: selectedContent,
        },
      }));
      setLoading(false);
    },
    [filter.startDate],
  );

  const fetchResults = React.useCallback(
    async (filter: MsearchMultisearchBody) => {
      try {
        setLoading(true);
        const res: any = await findContentWithElasticsearch(filter, false);
        setContent(res.hits.hits.map((h: { _source: IContentModel }) => h._source));
      } catch {
      } finally {
        setLoading(false);
      }
    },
    [findContentWithElasticsearch],
  );

  React.useEffect(() => {
    // stops invalid requests before filter is synced with date
    // wait for userinfo incase applying previously viewed filter
    if (!!featuredStoryActionId && !!userInfo) {
      fetchResults(
        generateQuery(
          filterFormat({
            ...createFilterSettings(
              filter.startDate ?? moment().startOf('day').toISOString(),
              filter.endDate ?? moment().endOf('day').toISOString(),
            ),
            actions: [getBooleanActionValue(featuredStoryActionId)],
            contentTypes: filter.contentTypes ?? [],
            mediaTypeIds: filter.mediaTypeIds ?? [],
            sourceIds: filter.sourceIds ?? [],
          }),
        ),
      );
    }
  }, [filter, fetchResults, userInfo, featuredStoryActionId]);

  const resetDateFilter = React.useCallback(() => {
    const defaultStartDate = moment().startOf('day').toISOString();
    const defaultEndDate = moment().endOf('day').toISOString();
    storeFilter({
      ...filter,
      startDate: defaultStartDate,
      endDate: defaultEndDate,
      dateOffset: undefined,
    });
  }, [filter, storeFilter]);

  const handleReset = React.useCallback(() => {
    setStateByDate((prevState) => {
      const newState: {
        [key: string]: {
          selected: IContentModel[];
          isSelectAllChecked: boolean;
          [key: string]: any;
        };
      } = {};
      for (const dateKey in prevState) {
        if (prevState[dateKey].selected.length > 0) {
          newState[dateKey] = {
            ...prevState[dateKey],
            selected: [],
            isSelectAllChecked: false,
          };
        } else {
          newState[dateKey] = prevState[dateKey];
        }
      }
      return newState;
    });
    resetDateFilter();
  }, [resetDateFilter]);

  const handleSelectAll = React.useCallback(
    (e: React.ChangeEvent<HTMLInputElement>) => {
      const dateKey = filter.startDate || moment().startOf('day').toISOString();
      setStateByDate((prevState) => ({
        ...prevState,
        [dateKey]: {
          ...prevState[dateKey],
          selected: e.target.checked ? content : [],
          isSelectAllChecked: e.target.checked,
        },
      }));
    },
    [content, filter.startDate],
  );

  const dateKey = filter.startDate || moment().startOf('day').toISOString();
  const currentSelected = stateByDate[dateKey]?.selected || [];
  const currentIsSelectAllChecked = stateByDate[dateKey]?.isSelectAllChecked || false;
  const allSelectedContent = Object.values(stateByDate).flatMap((state) => state.selected);

  return (
    <styled.Home>
      <Row>
        <ContentListActionBar
          content={allSelectedContent}
          onSelectAll={handleSelectAll}
          isSelectAllChecked={currentIsSelectAllChecked}
          onClear={() =>
            setStateByDate((prevState) => ({
              ...prevState,
              [dateKey]: {
                ...prevState[dateKey],
                selected: [],
              },
            }))
          }
          onReset={handleReset}
        />
      </Row>
      <DateFilter filter={filter} storeFilter={storeFilter} />
      <Show visible={loading}>
        <Loading />
      </Show>
      <ContentList
        onContentSelected={handleContentSelected}
        showDate
        showSeries
        showTime
        selected={currentSelected}
        content={content}
      />
    </styled.Home>
  );
};
