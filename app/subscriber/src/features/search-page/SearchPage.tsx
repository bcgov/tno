import { MsearchMultisearchBody } from '@elastic/elasticsearch/lib/api/types';
import { BasicSearch } from 'components/basic-search';
import { ContentList, ViewOptions } from 'components/content-list';
import { PageSection } from 'components/section';
import { ContentListActionBar } from 'components/tool-bar';
import { useElastic } from 'features/my-searches/hooks';
import { IContentSearchResult } from 'features/utils/interfaces';
import React from 'react';
import { FaBookmark } from 'react-icons/fa6';
import { useParams } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useContent, useFilters, useLookup } from 'store/hooks';
import { useProfileStore } from 'store/slices';
import { Col, IContentModel, Loading, Row, Show } from 'tno-core';

import { AdvancedSearch } from './components';
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
      search: { filter },
    },
    { findContentWithElasticsearch, storeSearchFilter },
  ] = useContent();
  const [{ frontPageImagesMediaTypeId }] = useLookup();
  const genQuery = useElastic();
  const [, { getFilter }] = useFilters();
  const [{ filter: activeFilter }, { storeFilter }] = useProfileStore();

  const [content, setContent] = React.useState<IContentSearchResult[]>([]);
  const [selected, setSelected] = React.useState<IContentModel[]>([]);
  const [isLoading, setIsLoading] = React.useState(false);
  const [init, setInit] = React.useState(true); // React hooks are horrible...

  const filterId = id ? parseInt(id) : 0;

  React.useEffect(() => {
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
  }, [activeFilter, getFilter, filterId, init, storeFilter, storeSearchFilter]);

  const fetchResults = React.useCallback(
    async (filter: MsearchMultisearchBody, searchUnpublished: boolean) => {
      try {
        setIsLoading(true);
        const res: any = await findContentWithElasticsearch(filter, searchUnpublished, 'search');
        setContent(res.hits.hits.map((h: { _source: IContentModel }) => h._source));
        if (res.hits.total.value >= 500)
          toast.warn(
            'Search returned 500+ results, only showing first 500. Please consider refining your search.',
          );
      } catch {
      } finally {
        setIsLoading(false);
      }
    },
    [findContentWithElasticsearch],
  );

  const handleContentSelected = React.useCallback((content: IContentModel[]) => {
    setSelected(content);
  }, []);

  React.useEffect(() => {
    // Need to wait until front page images are in redux store before making a request.
    if (frontPageImagesMediaTypeId) {
      const settings = filterFormat(filter);
      const query = genQuery(settings);
      fetchResults(query, filter.searchUnpublished);
    }
    // Only execute this on page load.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [frontPageImagesMediaTypeId]);

  const handleSearch = React.useCallback(async () => {
    const settings = filterFormat(filter);
    const query = genQuery(settings);
    fetchResults(query, filter.searchUnpublished);
  }, [fetchResults, filter, genQuery]);

  return (
    <styled.SearchPage>
      <Row className="search-container">
        {/* LEFT SIDE */}
        <Show visible={showAdvanced}>
          <Col className="adv-search-container">
            <AdvancedSearch onSearch={() => handleSearch()} />
          </Col>
        </Show>
        {/* RIGHT SIDE */}
        <Col className={showAdvanced ? 'result-container' : 'result-container-full'}>
          <Show visible={!showAdvanced}>
            <BasicSearch onSearch={() => handleSearch()} />
          </Show>
          <PageSection
            header={
              <Row className="header-row">
                <h1 className="title">{`Search Results`}</h1>
                <ViewOptions />
              </Row>
            }
          >
            <ContentListActionBar
              content={selected}
              onSelectAll={(e) => (e.target.checked ? setSelected(content) : setSelected([]))}
              className="search"
            />
            <Show visible={!!activeFilter}>
              <div className="viewed-name ">
                <FaBookmark />
                <div className="filter-name">{activeFilter?.name}</div>
              </div>
            </Show>
            <Show visible={!content.length}>
              <Row className="helper-text" justifyContent="center">
                Please refine search criteria and click "search".
              </Row>
            </Show>
            <ContentList
              onContentSelected={handleContentSelected}
              content={content}
              selected={selected}
              showDate
              styleOnSettings
              scrollWithin
            />
            {isLoading && <Loading />}
          </PageSection>
        </Col>
      </Row>
    </styled.SearchPage>
  );
};
