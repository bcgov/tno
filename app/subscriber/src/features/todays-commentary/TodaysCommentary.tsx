import { ContentList } from 'components/content-list';
import { DateFilter } from 'components/date-filter';
import { ContentListActionBar } from 'components/tool-bar';
import { useActionFilters } from 'features/search-page/hooks';
import { filterFormat } from 'features/search-page/utils';
import { castToSearchResult } from 'features/utils';
import { IContentSearchResult } from 'features/utils/interfaces';
import moment from 'moment';
import React from 'react';
import { useContent, useSettings } from 'store/hooks';
import { generateQuery, IContentModel, Loading, Show } from 'tno-core';

import * as styled from './styled';

/** Component that displays commentary defaulting to today's date and adjustable via a date filter. */
export const TodaysCommentary: React.FC = () => {
  const [
    {
      todaysCommentary: { filter },
    },
    { findContentWithElasticsearch, storeTodaysCommentaryFilter: storeFilter },
  ] = useContent();
  const getActionFilters = useActionFilters();
  const { commentaryActionId } = useSettings();
  const [content, setContent] = React.useState<IContentSearchResult[]>([]);
  const [selected, setSelected] = React.useState<IContentModel[]>([]);
  const [isSelectAllChecked, setIsSelectAllChecked] = React.useState(false);
  const [loading, setLoading] = React.useState(false);
  const handleContentSelected = React.useCallback((content: IContentModel[]) => {
    setSelected(content);
    setLoading(false);
  }, []);

  React.useEffect(() => {
    if (commentaryActionId) {
      let actionFilters = getActionFilters();
      const commentaryAction = actionFilters.find((a) => a.id === commentaryActionId);
      setLoading(true);
      findContentWithElasticsearch(
        generateQuery(
          filterFormat({
            ...filter,
            actions: commentaryAction ? [commentaryAction] : [],
            startDate: filter.startDate ?? moment().startOf('day').toISOString(),
            endDate: filter.endDate ?? moment().endOf('day').toISOString(),
            searchUnpublished: false,
            size: 500,
          }),
        ),
        false,
      )
        .then((res) => {
          setContent(
            res.hits.hits.map((r) => {
              const content = r._source as IContentModel;
              return castToSearchResult(content);
            }),
          );
          setLoading(false);
        })
        .catch();
    }
  }, [commentaryActionId, filter, findContentWithElasticsearch, getActionFilters]);
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
    setSelected([]);
    resetDateFilter();
    setIsSelectAllChecked(false); // Uncheck the checkbox
  }, [resetDateFilter]);

  const handleSelectAll = React.useCallback(
    (e: React.ChangeEvent<HTMLInputElement>) => {
      setSelected(e.target.checked ? content : []);
      setIsSelectAllChecked(e.target.checked); // Update checkbox state
    },
    [content],
  );

  return (
    <styled.TodaysCommentary>
      <ContentListActionBar
        content={selected}
        onClear={() => setSelected([])}
        onSelectAll={handleSelectAll}
        isSelectAllChecked={isSelectAllChecked}
        onReset={handleReset}
      />
      <DateFilter filter={filter} storeFilter={storeFilter} />
      <Show visible={loading}>
        <Loading />
      </Show>
      <ContentList
        content={content}
        selected={selected}
        showSeries
        showDate
        showTime
        onContentSelected={handleContentSelected}
      />
    </styled.TodaysCommentary>
  );
};
