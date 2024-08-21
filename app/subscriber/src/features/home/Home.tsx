import { ContentList } from 'components/content-list';
import { DateFilter } from 'components/date-filter';
import { ContentListActionBar } from 'components/tool-bar';
import { PreviousResults } from 'features/previous-results';
import { getBooleanActionValue } from 'features/utils';
import moment from 'moment';
import React from 'react';
import { useContent, useFetchResults, useSettings } from 'store/hooks';
import { IContentModel, Loading, Row, Show } from 'tno-core';

import * as styled from './styled';

/**
 * Home component that will be rendered when the user is logged in.
 */
export const Home: React.FC = () => {
  const { currDateResults, prevDateResults, fetchResults, isLoading, setIsLoading } =
    useFetchResults();
  const [
    {
      home: { filter },
    },
    { storeHomeFilter: storeFilter },
  ] = useContent();

  const { featuredStoryActionId } = useSettings(true);
  const [stateByDate, setStateByDate] = React.useState<{
    [date: string]: { selected: IContentModel[]; isSelectAllChecked: boolean };
  }>({});

  React.useEffect(() => {
    if (!featuredStoryActionId || isLoading) return;
    fetchResults({
      ...filter,
      actions: [getBooleanActionValue(featuredStoryActionId)],
      startDate: filter.startDate ?? moment().startOf('day').toISOString(),
    }).then(() => {});
    // only want to fire when filter changes, or when the featured story action id changes
    // react does not like dependencies that are from a hook it seems.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [filter, featuredStoryActionId]);

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
    // only want to re-trigger when the start date changes
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
      <Show visible={isLoading}>
        <Loading />
      </Show>
      <ContentList
        onContentSelected={handleContentSelected}
        showDate
        showSeries
        showTime
        selected={currentSelected}
        content={currDateResults}
      />
      <Show visible={!currDateResults.length && !isLoading}>
        <PreviousResults
          currDateResults={currDateResults}
          prevDateResults={prevDateResults}
          loaded={!!featuredStoryActionId}
          filter={filter}
          storeFilter={storeFilter}
        />
      </Show>
    </styled.Home>
  );
};
