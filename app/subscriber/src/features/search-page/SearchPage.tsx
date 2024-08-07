import { BasicSearch } from 'components/basic-search';
import { ContentList, ViewOptions } from 'components/content-list';
import { DateFilter } from 'components/date-filter';
import { PageSection } from 'components/section';
import { ContentListActionBar } from 'components/tool-bar';
import { useElastic } from 'features/my-searches/hooks';
import { IContentSearchResult } from 'features/utils/interfaces';
import React from 'react';
import { FaBookmark } from 'react-icons/fa6';
import { useLocation, useParams } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useContent, useFilters, useLookup } from 'store/hooks';
import { useProfileStore } from 'store/slices';
import {
  Col,
  IContentModel,
  IFilterSettingsModel,
  Loading,
  Row,
  Show,
  useWindowSize,
} from 'tno-core';

import { AdvancedSearch } from './components';
import { PreviousResults } from './PreviousResults';
import { useSearchPageContext } from './SearchPageContext';
import * as styled from './styled';
import { filterFormat } from './utils';

export interface ISearchType {
  showAdvanced?: boolean;
}

// Simple component to display users search results
export const SearchPage: React.FC<ISearchType> = ({ showAdvanced }) => {
  const { id } = useParams();
  const [
    {
      search: { filter, content },
    },
    { findContentWithElasticsearch, storeSearchFilter },
  ] = useContent();
  const [{ frontPageImagesMediaTypeId }] = useLookup();
  const { width } = useWindowSize();
  const genQuery = useElastic();
  const [, { getFilter }] = useFilters();
  const [{ filter: activeFilter }, { storeFilter }] = useProfileStore();
  const { pathname } = useLocation();

  const [currDateResults, setCurrDateResults] = React.useState<IContentSearchResult[]>([]);
  const [prevDateResults, setPrevDateResults] = React.useState<IContentSearchResult[]>([]);
  const [selected, setSelected] = React.useState<IContentModel[]>([]);
  const [totalResults, setTotalResults] = React.useState(0);
  const [isLoading, setIsLoading] = React.useState(false);
  const { expanded } = useSearchPageContext();
  const [startDate, setStartDate] = React.useState<Date>(new Date());
  const [init, setInit] = React.useState(true); // React hooks are horrible...

  const [filterId, setFilterId] = React.useState(0);
  const [searchFilter, setSearchFilter] = React.useState<IFilterSettingsModel | null>(null);

  React.useEffect(() => {
    const parsedId = id ? parseInt(id) : 0;
    setFilterId(parsedId);
    // Fetch the active filter if required.
    if (filterId && init && activeFilter?.id !== filterId) {
      setInit(false);
      getFilter(filterId)
        .then((filter) => {
          storeFilter(filter);
          storeSearchFilter(filter.settings);
        })
        .catch(() => {});
    } else if (!filterId) {
      storeFilter(undefined);
    }
  }, [activeFilter, getFilter, filterId, init, storeFilter, storeSearchFilter, id]);

  const groupResults = React.useCallback(
    (res: any, currStartDate: Date, currEndDate: Date, prevStartDate: Date) => {
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
        } else {
          prevDateResults.push(h._source);
        }
      });
      setCurrDateResults(currDateResults);
      setPrevDateResults(prevDateResults);
      setTotalResults(currDateResults.length + prevDateResults.length);
      if (res.hits.total.value === 0) toast.warn('No results found.');
      if (res.hits.total.value >= 500)
        toast.warn(
          'Search returned 500+ results, only showing first 500. Please consider refining your search.',
        );
    },
    [],
  );

  const fetchResults = React.useCallback(
    async (filter: IFilterSettingsModel, storedContent?: any) => {
      try {
        setIsLoading(true);
        let newFilter = filter;
        if (filter.dateOffset !== undefined) {
          newFilter = {
            ...filter,
            dateOffset: filter.dateOffset + 7,
          };
        }
        const offSet = filter.dateOffset ? filter.dateOffset : 0;
        const dayInMs = 24 * 60 * 60 * 1000; // Hours*Minutes*Seconds*Milliseconds
        let offSetDate = new Date();
        offSetDate.setDate(offSetDate.getDate() - offSet);
        offSetDate.setHours(0, 0, 0);
        const currStartDate = filter.startDate ? new Date(filter.startDate) : offSetDate;
        const prevStartDate = new Date(currStartDate.getTime() - 7 * dayInMs);
        const currEndDate = filter.endDate
          ? new Date(filter.endDate)
          : new Date(currStartDate.getTime() + offSet * dayInMs - 1);
        currEndDate.setHours(23, 59, 59);
        setStartDate(currStartDate);
        if (filter.startDate && filter.endDate) {
          newFilter = {
            ...filter,
            dateOffset: undefined,
            startDate: prevStartDate.toISOString(),
            endDate: currEndDate.toISOString(),
          };
        }
        const settings = filterFormat(newFilter);
        const query = genQuery(settings);
        let res;
        if (!storedContent) {
          res = await findContentWithElasticsearch(query, filter.searchUnpublished, 'search');
        } else {
          res = storedContent;
        }

        groupResults(res, currStartDate, currEndDate, prevStartDate);
      } catch {
      } finally {
        setIsLoading(false);
      }
    },
    [findContentWithElasticsearch, genQuery, groupResults],
  );

  React.useEffect(() => {
    // only fetch this when there's no call to the elastic search
    if (isLoading || (id && !activeFilter)) return;
    fetchResults(filter, content);
    // Do not execute when changing the filters
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [content, fetchResults, id]);

  const handleContentSelected = React.useCallback((content: IContentModel[]) => {
    setSelected(content);
  }, []);

  React.useEffect(() => {
    // Do not want it to fire on initial load of a user clicking "go advanced"
    // Need to wait until front page images are in redux store before making a request.
    if (frontPageImagesMediaTypeId && !pathname.includes('advanced')) {
      fetchResults(filter);
    }
    // Only execute this on page load.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [frontPageImagesMediaTypeId]);

  const handleSearch = React.useCallback(async () => {
    fetchResults(filter);
  }, [fetchResults, filter]);

  const handleAdvancedSearch = React.useCallback(
    async (updatedFilterSettings: IFilterSettingsModel) => {
      fetchResults(updatedFilterSettings);
    },
    [fetchResults],
  );

  const executeSearch = React.useCallback(
    async (filter: IFilterSettingsModel) => {
      fetchResults(filter);
    },
    [fetchResults],
  );

  return (
    <styled.SearchPage expanded={expanded}>
      <Row className="search-container">
        {/* LEFT SIDE */}
        <Show visible={showAdvanced}>
          <Col className="adv-search-container">
            <AdvancedSearch onSearch={handleAdvancedSearch} setSearchFilter={setSearchFilter} />
          </Col>
        </Show>
        {/* RIGHT SIDE */}
        <Col className={showAdvanced ? 'result-container' : 'result-container-full'}>
          <Show visible={!showAdvanced && !!width && width > 900}>
            <BasicSearch onSearch={() => handleSearch()} />
          </Show>
          <PageSection
            header={
              <Col className="header-col">
                <Row className="header-row">
                  <div className="title">{`Search Results`}</div>
                  <ViewOptions />
                </Row>
                {!!totalResults && (
                  <p className="result-total">{`${totalResults} stories found`}</p>
                )}
              </Col>
            }
          >
            <ContentListActionBar
              content={selected}
              onClear={() => setSelected([])}
              onSelectAll={(e) =>
                e.target.checked ? setSelected(currDateResults) : setSelected([])
              }
              className="search"
            />
            <DateFilter
              filter={filter}
              storeFilter={storeSearchFilter}
              onChangeDate={executeSearch}
            />
            <br />
            <Show visible={!!activeFilter}>
              <div className="viewed-name ">
                <FaBookmark />
                <div className="filter-name">{activeFilter?.name}</div>
              </div>
            </Show>
            <ContentList
              onContentSelected={handleContentSelected}
              content={currDateResults}
              selected={selected}
              showDate
              showTime
              showSeries
              scrollWithin
              filter={searchFilter ?? undefined}
            />
            <Show visible={!currDateResults.length}>
              <PreviousResults
                currDateResults={currDateResults}
                prevDateResults={prevDateResults}
                startDate={startDate}
                setResults={setPrevDateResults}
                executeSearch={executeSearch}
              />
            </Show>
            {isLoading && <Loading />}
          </PageSection>
        </Col>
      </Row>
    </styled.SearchPage>
  );
};
