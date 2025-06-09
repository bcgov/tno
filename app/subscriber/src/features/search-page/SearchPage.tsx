import {
  IndicesValidateQueryResponse,
  KnnSearchResponse,
} from '@elastic/elasticsearch/lib/api/types';
import { BasicSearch } from 'components/basic-search';
import { ContentList, ViewOptions } from 'components/content-list';
import { DateFilter } from 'components/date-filter';
import { FilterOptions } from 'components/media-type-filters';
import { PageSection } from 'components/section';
import { ContentListActionBar } from 'components/tool-bar';
import { useElastic } from 'features/my-searches/hooks';
import { castToSearchResult } from 'features/utils';
import { IContentSearchResult } from 'features/utils/interfaces';
import moment, { Moment } from 'moment';
import React from 'react';
import { FaBookmark, FaCaretLeft, FaCaretRight } from 'react-icons/fa6';
import { useLocation, useParams } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useApp, useContent, useFilters, useLookup } from 'store/hooks';
import { useProfileStore } from 'store/slices';
import {
  Col,
  IContentModel,
  IFilterSettingsModel,
  Loader,
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
  showDate?: boolean;
}

// Simple component to display users search results
export const SearchPage: React.FC<ISearchType> = ({ showAdvanced, showDate: initShowDate }) => {
  const { id } = useParams();
  const [
    {
      search: { filter, content },
      searchResults: { filter: secondaryFilter },
    },
    {
      findContentWithElasticsearch,
      storeSearchFilter,
      storeSearchResultsFilter,
      validateElasticsearchQuery,
    },
  ] = useContent();
  const [{ frontPageImagesMediaTypeId }] = useLookup();
  const { width } = useWindowSize();
  const genQuery = useElastic();
  const [, { getFilter }] = useFilters();
  const [{ from, filter: activeFilter }, { storeFrom, storeFilter }] = useProfileStore();
  const { pathname } = useLocation();
  const [{ requests }] = useApp();
  const [queryValidateResult, setQueryValidateResult] = React.useState<string[]>(['']);

  const [init, setInit] = React.useState(true); // React hooks are horrible...
  const [currDateResults, setCurrDateResults] = React.useState<IContentSearchResult[]>([]);
  const [prevDateResults, setPrevDateResults] = React.useState<IContentSearchResult[]>([]);
  const [selected, setSelected] = React.useState<IContentModel[]>([]);
  const { expanded } = useSearchPageContext();
  const [startDate, setStartDate] = React.useState<Moment>(moment());
  const [filterId, setFilterId] = React.useState(0);
  const [searchFilter, setSearchFilter] = React.useState<IFilterSettingsModel | null>(null);
  const [showResults, setShowResults] = React.useState(false);

  const [dateVisible, setDateVisible] = React.useState(initShowDate); // show/hide the date navigation component.
  const [selectedDate, setSelectedDate] = React.useState<string | undefined>(); // The selected date for the navigation component.

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

  React.useEffect(() => {
    setDateVisible(initShowDate);
  }, [initShowDate]);

  const groupResults = React.useCallback(
    (res: KnnSearchResponse<IContentModel>, currStartDate: Moment, groupStoredContent: boolean) => {
      const currDateResults: IContentSearchResult[] = [],
        prevDateResults: IContentSearchResult[] = [];
      res.hits.hits.forEach((h) => {
        if (!h._source) return;
        const content = castToSearchResult(h._source);
        const resDate = moment(content.publishedOn);
        if (resDate >= currStartDate) {
          // result occurred during currently selected date
          currDateResults.push(content);
        } else {
          prevDateResults.push(content);
        }
      });
      setCurrDateResults(currDateResults);
      setPrevDateResults(prevDateResults);
      if (!groupStoredContent) {
        if (
          (typeof res.hits.total === 'number' && res.hits.total === 0) ||
          (res.hits.total !== undefined &&
            typeof res.hits.total !== 'number' &&
            'value' in res.hits.total &&
            res.hits.total.value === 0)
        )
          toast.warn('No results found.');
        if (currDateResults.length >= 500)
          toast.warn(
            'Search returned 500+ results, only showing first 500. Please consider refining your search.',
          );
      }
    },
    [],
  );

  const parseValidationResults = (data: IndicesValidateQueryResponse | undefined) => {
    if (!data?.valid) {
      const error = data?.explanations?.find((x) => x.error)?.error;
      const regex = / at line \d+, column \d+/;
      const found = (error ?? '').match(regex);
      if (found) {
        return [`ERROR: ${found[0]}`];
      }
      return ['--Failed to parse query--'];
    }
    return [''];
  };

  const validationResults = React.useCallback((res: IndicesValidateQueryResponse | undefined) => {
    const parsedResult = parseValidationResults(res);
    setQueryValidateResult(parsedResult);
  }, []);

  // Check the secondary filter (FilterOptions component) and merge the options according to each search criteria.
  const mergeFilters = React.useCallback(
    (currentFilter: IFilterSettingsModel) => {
      const newFilter = { ...currentFilter };
      if (newFilter.contentTypes) {
        const contentTypesArray = [...newFilter.contentTypes];
        secondaryFilter.contentTypes?.forEach((c) => {
          contentTypesArray.push(c);
        });
        newFilter.contentTypes = contentTypesArray;
      }
      if (
        newFilter.mediaTypeIds &&
        secondaryFilter.mediaTypeIds &&
        secondaryFilter.mediaTypeIds.length > 0
      ) {
        newFilter.mediaTypeIds = secondaryFilter.mediaTypeIds;
      }
      if (
        newFilter.sourceIds &&
        secondaryFilter.sourceIds &&
        secondaryFilter.sourceIds.length > 0
      ) {
        newFilter.sourceIds = secondaryFilter.sourceIds;
      }
      return newFilter;
    },
    [secondaryFilter],
  );

  const fetchResults = React.useCallback(
    async (
      filter: IFilterSettingsModel,
      storedContent?: KnnSearchResponse<IContentModel>,
      oneDay: boolean = false,
    ) => {
      try {
        setShowResults(false);
        let newFilter = filter;
        if (filter.dateOffset !== undefined) {
          newFilter = {
            ...filter,
            dateOffset: filter.dateOffset + 7,
          };
        }
        const offset = filter.dateOffset ?? 0;

        const offsetDate =
          offset <= 2 ? moment().add(offset * 24 * -1, 'hour') : moment().add(offset * -1, 'day');
        if (offset > 2 || offset === 0) offsetDate.startOf('day');

        // if no date offset/ start date this means the user is looking for all content
        // unix epoch used as a default start date for all content
        const currStartDate = filter.startDate
          ? moment(filter.startDate)
          : filter.dateOffset !== undefined
          ? moment(offsetDate)
          : moment(1432252800); // 1970
        const prevStartDate = moment(currStartDate).add(-7, 'day');

        const currEndDate = filter.endDate ? moment(filter.endDate) : moment();
        currEndDate.endOf('day');

        setStartDate(currStartDate);
        if (filter.startDate && filter.endDate) {
          newFilter = {
            ...filter,
            dateOffset: undefined,
            startDate: oneDay ? currStartDate.toISOString() : prevStartDate.toISOString(),
            endDate: currEndDate.toISOString(),
          };
        }
        newFilter = mergeFilters(newFilter);
        const settings = filterFormat(newFilter);
        const query = genQuery(settings);
        if (!query) return;
        let res;
        let groupStoredContent = false;
        if (!storedContent) {
          storeFrom(query.from ?? 0);
          res = await findContentWithElasticsearch(query, filter.searchUnpublished, 'search');
        } else {
          res = storedContent;
          groupStoredContent = true;
        }
        groupResults(res, currStartDate, groupStoredContent);
      } catch {
      } finally {
        setShowResults(true);
      }
    },
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [findContentWithElasticsearch, genQuery, groupResults, mergeFilters],
  );

  const validateQueryString = React.useCallback(
    async (
      filter: IFilterSettingsModel,
      storedContent?: KnnSearchResponse<IContentModel>,
      oneDay: boolean = false,
    ) => {
      try {
        const settings = filterFormat(filter);
        const query = genQuery(settings);
        let res;
        if (!storedContent) {
          storeFrom(query?.from ?? 0);
          res = await validateElasticsearchQuery(query ?? {}, filter.searchUnpublished, 'headline');
          validationResults(res);
        }
      } catch {
      } finally {
      }
    },
    [genQuery, storeFrom, validateElasticsearchQuery, validationResults],
  );

  React.useEffect(() => {
    // only fetch this when there's no call to the elastic search
    if (id && !activeFilter) return;
    fetchResults(filter, content);
    // Do not execute when changing the filters
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [content, fetchResults, id]);

  React.useEffect(() => {
    storeSearchResultsFilter(secondaryFilter);
    fetchResults(filter);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [secondaryFilter]);

  React.useEffect(() => {
    // Do not want it to fire on initial load of a user clicking "go advanced"
    // Need to wait until front page images are in redux store before making a request.
    if (frontPageImagesMediaTypeId && !pathname.includes('advanced')) {
      fetchResults(filter);
    }
    // Only execute this on page load.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [frontPageImagesMediaTypeId]);

  const handleContentSelected = React.useCallback((content: IContentModel[]) => {
    setSelected(content);
  }, []);

  return (
    <styled.SearchPage expanded={expanded}>
      <Row className="search-container">
        {/* LEFT SIDE */}
        <Show visible={showAdvanced}>
          <Col className="adv-search-container">
            <AdvancedSearch
              onSearch={fetchResults}
              setSearchFilter={setSearchFilter}
              validateQuery={validateQueryString}
              queryValidateResult={queryValidateResult}
            />
          </Col>
        </Show>
        {/* RIGHT SIDE */}
        <Col className={showAdvanced ? 'result-container' : 'result-container-full'}>
          <Show visible={!showAdvanced && !!width && width > 900}>
            <BasicSearch onSearch={() => fetchResults(filter)} />
          </Show>
          <PageSection
            header={
              <Col className="header-col">
                <Row className="header-row">
                  <div className="title">{`Search Results`}</div>
                  <FilterOptions filterStoreName={'searchResults'} />
                  <ViewOptions />
                </Row>
                <Row gap="1rem" className="search-results">
                  {!!currDateResults.length && (
                    <p className="result-total">{`${currDateResults.length} stories found`}</p>
                  )}
                  <Row className="search-results-paging">
                    {!!from && (
                      <FaCaretLeft
                        className="btn"
                        title="Previous Page"
                        onClick={() => {
                          const position = from ? from - filter.size : 0;
                          const newFilter = {
                            ...filter,
                            from: position > 0 ? position : undefined,
                          };
                          fetchResults(newFilter);
                        }}
                      />
                    )}
                    p.
                    {from >= filter.size ? Math.floor(from / filter.size) + 1 : 1}
                    {filter.size === currDateResults.length && (
                      <FaCaretRight
                        className="btn"
                        title="Next Page"
                        onClick={() => {
                          const position = from + filter.size;
                          const newFilter = { ...filter, from: position };
                          fetchResults(newFilter);
                        }}
                      />
                    )}
                  </Row>
                </Row>
              </Col>
            }
          >
            <ContentListActionBar
              content={selected}
              onClear={() => setSelected([])}
              onSelectAll={(e) => {
                const values = currDateResults.map((c) => c.id);
                const oldSelected = selected.filter((s) => !values.includes(s.id));
                e.target.checked
                  ? setSelected([...selected, ...currDateResults])
                  : setSelected(oldSelected);
              }}
              className="search"
            />
            <Show visible={dateVisible}>
              <DateFilter
                date={selectedDate}
                onChangeDate={(start, end) => {
                  setSelectedDate(start);
                  fetchResults(
                    { ...filter, startDate: start, endDate: end, dateOffset: undefined },
                    undefined,
                    true,
                  );
                }}
              />
            </Show>
            <br />
            <Show visible={!!activeFilter}>
              <div className="viewed-name ">
                <FaBookmark />
                <div className="filter-name">{activeFilter?.name}</div>
              </div>
            </Show>
            <Show visible={showResults && !!currDateResults.length}>
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
            </Show>
            <Show visible={!currDateResults.length}>
              <PreviousResults
                currDateResults={currDateResults}
                prevDateResults={prevDateResults}
                startDate={startDate}
                setResults={setPrevDateResults}
                executeSearch={fetchResults}
              />
            </Show>
            <Loader visible={requests.some((r) => r.url === 'find-contents-with-elasticsearch')} />
          </PageSection>
        </Col>
      </Row>
    </styled.SearchPage>
  );
};
