import { MsearchMultisearchBody } from '@elastic/elasticsearch/lib/api/types';
import { ContentList } from 'components/content-list';
import { DateFilter } from 'components/date-filter';
import { ContentListActionBar } from 'components/tool-bar';
import { IContentSearchResult } from 'features/utils/interfaces';
import React from 'react';
import { useContent, useSettings } from 'store/hooks';
import { generateQuery, IContentModel, Loading, Show } from 'tno-core';

import { PreviousResults } from './PreviousResults';
import * as styled from './styled';

interface IFilterMediaProps {
  loaded?: boolean;
  selected: IContentModel[];
  setSelected: React.Dispatch<React.SetStateAction<IContentModel[]>>;
  onReset: () => void;
}

export const FilterMedia: React.FC<IFilterMediaProps> = ({
  loaded,
  selected,
  setSelected,
  onReset,
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

  const handleContentSelected = React.useCallback((content: IContentModel[]) => {
    setSelected(content);
    setIsLoading(false);
  }, []);

  if (!loaded) return <>Loading</>;

  return (
    <styled.FilterMedia className="results-side">
      <ContentListActionBar
        content={selected}
        onSelectAll={(e) => (e.target.checked ? setSelected(currDateResults) : setSelected([]))}
        onClear={() => setSelected([])}
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
        selected={selected}
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
