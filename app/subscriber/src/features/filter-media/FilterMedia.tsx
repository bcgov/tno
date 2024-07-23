import { MsearchMultisearchBody } from '@elastic/elasticsearch/lib/api/types';
import { ContentList } from 'components/content-list';
import { DateFilter } from 'components/date-filter';
import { ContentListActionBar } from 'components/tool-bar';
import { IContentSearchResult } from 'features/utils/interfaces';
import moment from 'moment';
import React from 'react';
import { useContent, useSettings } from 'store/hooks';
import { generateQuery, IContentModel, Loading, Show } from 'tno-core';

import { PreviousResults } from './PreviousResults';
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
    { findContentWithElasticsearch, storeMediaTypeFilter: storeFilter },
  ] = useContent();
  const { mediaTypesIdsAllSources } = useSettings();

  const [currDateResults, setCurrDateResults] = React.useState<IContentSearchResult[]>([]);
  const [prevDateResults, setPrevDateResults] = React.useState<IContentSearchResult[]>([]);
  const [isLoading, setIsLoading] = React.useState(true);

  const fetchResults = React.useCallback(
    async (requestFilter: MsearchMultisearchBody) => {
      if (!filter.startDate) return;
      const dayInMillis = 24 * 60 * 60 * 1000; // Hours*Minutes*Seconds*Milliseconds
      const currStartDate = new Date(filter.startDate);
      const prevStartDate = new Date(currStartDate.getTime() - 5 * dayInMillis);
      const currEndDate = new Date(currStartDate.getTime() + dayInMillis - 1);
      const query = generateQuery({
        ...filter,
        mediaTypeIds: filter.mediaTypeIds ?? [],
        sourceIds: mediaTypesIdsAllSources?.some((id) => filter.mediaTypeIds?.includes(id))
          ? []
          : filter.sourceIds ?? [],
        seriesIds: filter.seriesIds ?? [],
        startDate: prevStartDate.toISOString(),
        endDate: currEndDate.toISOString(),
      });
      try {
        const emptyFilters = filter.sourceIds?.length === 0 && filter.seriesIds?.length === 0;
        if (emptyFilters) {
          setCurrDateResults([]);
          setPrevDateResults([]);
          return;
        }
        setIsLoading(true);
        const res: any = await findContentWithElasticsearch(query, false);
        const currDateResults: IContentSearchResult[] = [],
          prevDateResults: IContentSearchResult[] = [];

        res.hits.hits.forEach((h: { _source: IContentSearchResult }) => {
          const resDate = new Date(h._source.publishedOn);
          if (
            resDate.getTime() >= currStartDate.getTime() &&
            resDate.getTime() <= currEndDate.getTime()
          ) {
            // result occurred during currently selected date
            currDateResults.push(h._source);
          } else if (
            // result occurred sometime in past 5 days
            resDate.getTime() >= prevStartDate.getTime() &&
            resDate.getTime() <= currEndDate.getTime()
          ) {
            prevDateResults.push(h._source);
          }
        });
        setCurrDateResults(currDateResults);
        setPrevDateResults(prevDateResults);
      } catch {
      } finally {
        setIsLoading(false);
      }
    },
    // only run on filter change
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [filter],
  );

  React.useEffect(() => {
    // stops invalid requests before data is loaded or filter is synced with date
    if (!loaded || !filter.startDate) return;
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
      <DateFilter loaded={loaded} filter={filter} storeFilter={storeFilter} />
      <Show visible={isLoading}>
        <Loading />
      </Show>
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
          setResults={setPrevDateResults}
        />
      </Show>
    </styled.FilterMedia>
  );
};
