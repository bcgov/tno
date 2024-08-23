import { ContentList } from 'components/content-list';
import { DateFilter } from 'components/date-filter';
import { ContentListActionBar } from 'components/tool-bar';
import { PreviousResults } from 'features/previous-results';
import { useActionFilters } from 'features/search-page/hooks';
import moment from 'moment';
import React from 'react';
import { useContent, useFetchResults, useSettings } from 'store/hooks';
import { IContentModel, Loading, Show } from 'tno-core';

import * as styled from './styled';

/** Component that displays commentary defaulting to today's date and adjustable via a date filter. */
export const TodaysCommentary: React.FC = () => {
  const { setIsLoading, fetchResults, prevDateResults, currDateResults, isLoading } =
    useFetchResults();
  const [
    {
      todaysCommentary: { filter },
    },
    { findContentWithElasticsearch, storeTodaysCommentaryFilter: storeFilter },
  ] = useContent();
  const getActionFilters = useActionFilters();
  const { commentaryActionId } = useSettings();
  const [stateByDate, setStateByDate] = React.useState<{
    [date: string]: { selected: IContentModel[]; isSelectAllChecked: boolean };
  }>({});

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
      setIsLoading(false);
    },
    // only update when the start date changes
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [filter.startDate],
  );

  React.useEffect(() => {
    if (commentaryActionId) {
      let actionFilters = getActionFilters();
      const commentaryAction = actionFilters.find((a) => a.id === commentaryActionId);
      setIsLoading(true);
      commentaryAction &&
        fetchResults({
          ...filter,
          actions: [commentaryAction],
          startDate: filter.startDate ?? moment().startOf('day').toISOString(),
          searchUnpublished: false,
          sort: [{ publishedOn: 'desc' }],
        });
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
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
          selected: e.target.checked ? currDateResults : [],
          isSelectAllChecked: e.target.checked,
        },
      }));
    },
    [currDateResults, filter.startDate],
  );

  const dateKey = filter.startDate || moment().startOf('day').toISOString();
  const currentSelected = stateByDate[dateKey]?.selected || [];
  const currentIsSelectAllChecked = stateByDate[dateKey]?.isSelectAllChecked || false;
  const allSelectedContent = Object.values(stateByDate).flatMap((state) => state.selected);

  return (
    <styled.TodaysCommentary>
      <ContentListActionBar
        content={allSelectedContent}
        onClear={() =>
          setStateByDate((prevState) => ({
            ...prevState,
            [dateKey]: {
              ...prevState[dateKey],
              selected: [],
            },
          }))
        }
        onSelectAll={handleSelectAll}
        isSelectAllChecked={currentIsSelectAllChecked}
        onReset={handleReset}
      />
      <DateFilter filter={filter} storeFilter={storeFilter} />
      <Show visible={isLoading}>
        <Loading />
      </Show>
      <ContentList
        content={currDateResults}
        selected={currentSelected}
        showSeries
        showDate
        showTime
        onContentSelected={handleContentSelected}
      />
      <Show visible={!!prevDateResults.length}>
        <PreviousResults
          currDateResults={currDateResults}
          prevDateResults={prevDateResults}
          loaded={!!commentaryActionId}
          filter={filter}
          storeFilter={storeFilter}
        />
      </Show>
    </styled.TodaysCommentary>
  );
};
