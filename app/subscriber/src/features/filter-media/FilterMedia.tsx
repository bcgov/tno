import { ContentList } from 'components/content-list';
import { DateFilter } from 'components/date-filter';
import { ContentListActionBar } from 'components/tool-bar';
import { PreviousResults } from 'features/previous-results';
import moment from 'moment';
import React from 'react';
import { useApp, useContent, useFetchResults } from 'store/hooks';
import { IContentModel, Show } from 'tno-core';

import * as styled from './styled';

interface IFilterMediaProps {
  loaded?: boolean;
  onReset: () => void;
  setStateByDate: React.Dispatch<
    React.SetStateAction<{
      [date: string]: {
        [mediaType: string]: { selected: IContentModel[]; isSelectAllChecked: boolean };
      };
    }>
  >;
  stateByDate: {
    [date: string]: {
      [mediaType: string]: { selected: IContentModel[]; isSelectAllChecked: boolean };
    };
  };
}

export const FilterMedia: React.FC<IFilterMediaProps> = ({
  loaded,
  onReset,
  setStateByDate,
  stateByDate,
}) => {
  const [
    {
      mediaType: { filter },
    },
    { storeMediaTypeFilter: storeFilter },
  ] = useContent();
  const [{ userInfo }] = useApp();

  const { currDateResults, prevDateResults, fetchResults } = useFetchResults();
  React.useEffect(() => {
    // stops invalid requests before data is loaded or filter is synced with date
    if (!loaded || !filter.startDate || !userInfo) return;
    fetchResults(filter);
    // only run on filter change
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [filter]);

  const handleContentSelected = React.useCallback(
    (selectedContent: IContentModel[]) => {
      const dateKey = filter.startDate || moment().startOf('day').toISOString();
      const mediaTypeId = filter.mediaTypeIds?.[0]?.toString() ?? '';
      setStateByDate((prevState) => ({
        ...prevState,
        [dateKey]: {
          ...prevState[dateKey],
          [mediaTypeId]: {
            selected: selectedContent,
            isSelectAllChecked: prevState[dateKey]?.[mediaTypeId]?.isSelectAllChecked ?? false,
          },
        },
      }));
    },
    [filter.mediaTypeIds, filter.startDate, setStateByDate],
  );

  const handleSelectAll = React.useCallback(
    (e: React.ChangeEvent<HTMLInputElement>) => {
      const dateKey = filter.startDate || moment().startOf('day').toISOString();
      const mediaTypeId = filter.mediaTypeIds?.[0]?.toString() ?? '';
      setStateByDate((prevState) => ({
        ...prevState,
        [dateKey]: {
          ...prevState[dateKey],
          [mediaTypeId]: {
            selected: e.target.checked ? currDateResults : [],
            isSelectAllChecked: e.target.checked,
          },
        },
      }));
    },
    [currDateResults, filter.mediaTypeIds, filter.startDate, setStateByDate],
  );

  if (!loaded) return <>Loading</>;

  const dateKey = filter.startDate || moment().startOf('day').toISOString();
  const mediaTypeId = filter.mediaTypeIds?.[0]?.toString() ?? '';
  const currentSelected = stateByDate[dateKey]?.[mediaTypeId]?.selected || [];
  const currentIsSelectAllChecked =
    stateByDate[dateKey]?.[mediaTypeId]?.isSelectAllChecked || false;
  const allSelectedContent = Object.values(stateByDate).flatMap((dateEntry) =>
    Object.values(dateEntry).flatMap((mediaTypeEntry) => mediaTypeEntry.selected),
  );

  return (
    <styled.FilterMedia className="results-side">
      <ContentListActionBar
        content={allSelectedContent}
        onSelectAll={handleSelectAll}
        onClear={() =>
          setStateByDate((prevState) => ({
            ...prevState,
            [dateKey]: {
              ...prevState[dateKey],
              [mediaTypeId]: {
                selected: [],
                isSelectAllChecked: false,
              },
            },
          }))
        }
        isSelectAllChecked={currentIsSelectAllChecked}
        onReset={onReset}
      />
      <DateFilter
        date={filter.startDate}
        onChangeDate={(start, end) =>
          storeFilter({ ...filter, startDate: start, endDate: end, dateOffset: undefined })
        }
      />
      <ContentList
        onContentSelected={handleContentSelected}
        content={currDateResults}
        showDate
        showTime
        showSeries
        selected={currentSelected}
      />
      <Show visible={!currDateResults.length}>
        <PreviousResults
          loaded={loaded}
          currDateResults={currDateResults}
          prevDateResults={prevDateResults}
          filter={filter}
          storeFilter={storeFilter}
        />
      </Show>
    </styled.FilterMedia>
  );
};
