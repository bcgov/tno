import { ContentList } from 'components/content-list';
import { DateFilter } from 'components/date-filter';
import { ContentListActionBar } from 'components/tool-bar';
import { PreviousResults } from 'features/previous-results';
import { useActionFilters } from 'features/search-page/hooks';
import moment from 'moment';
import React from 'react';
import { useContent, useFetchResults, useSettings } from 'store/hooks';
import { IContentModel, Show } from 'tno-core';

import * as styled from './styled';

/** Component that displays top stories defaulting to today's date and adjustable via a date filter. */
export const TopStories: React.FC = () => {
  const { setIsLoading, fetchResults, prevDateResults, currDateResults } = useFetchResults();
  const [
    {
      topStories: { filter },
    },
    { storeTopStoriesFilter: storeFilter },
  ] = useContent();
  const getActionFilters = useActionFilters();
  const { topStoryActionId, isReady } = useSettings();

  const [stateByDate, setStateByDate] = React.useState<{
    [date: string]: { selected: IContentModel[]; isSelectAllChecked: boolean };
  }>({});

  React.useEffect(() => {
    // stops invalid requests before filter is synced with date
    if (isReady) {
      let actionFilters = getActionFilters();
      const topStoryAction = actionFilters.find((a) => a.id === topStoryActionId);
      topStoryAction &&
        fetchResults({
          ...filter,
          actions: [topStoryAction],
          startDate: filter.startDate ?? moment().startOf('day').toISOString(),
          size: 500,
          sort: [{ publishedOn: 'desc' }],
        }).catch();
    }
    // react does not like dependencies that are from a hook
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [filter, getActionFilters, isReady, topStoryActionId]);

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
    // only want to re-run when the start date changes
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [filter.startDate],
  );

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
    <styled.TopStories>
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
      <ContentList
        content={currDateResults}
        onContentSelected={handleContentSelected}
        showTime
        showDate
        showSeries
        selected={currentSelected}
      />
      <Show visible={!currDateResults.length}>
        <PreviousResults
          currDateResults={currDateResults}
          loaded={!!topStoryActionId}
          prevDateResults={prevDateResults}
          filter={filter}
          storeFilter={storeFilter}
        />
      </Show>
    </styled.TopStories>
  );
};
