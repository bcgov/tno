import { MsearchMultisearchBody } from '@elastic/elasticsearch/lib/api/types';
import { BasicSearch } from 'components/basic-search';
import { PageSection } from 'components/section';
import { Sentiment } from 'components/sentiment';
import { ContentListActionBar } from 'components/tool-bar';
import { useElastic } from 'features/my-searches/hooks';
import { determinePreview } from 'features/utils';
import React from 'react';
import { FaPlay, FaStop } from 'react-icons/fa';
import { FaBookmark } from 'react-icons/fa6';
import { useNavigate, useParams } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useContent, useFilters, useLookup } from 'store/hooks';
import { useProfileStore } from 'store/slices';
import { Checkbox, Col, IContentModel, Loading, Row, Show, toQueryString } from 'tno-core';

import { AdvancedSearch } from './components';
import { Player } from './player/Player';
import * as styled from './styled';
import { filterFormat, formatSearch } from './utils';

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
  const navigate = useNavigate();
  const [{ actions, frontPageImagesMediaTypeId }] = useLookup();
  const genQuery = useElastic();
  const [, { getFilter }] = useFilters();
  const [{ filter: activeFilter }, { storeFilter }] = useProfileStore();

  const [content, setContent] = React.useState<IContentModel[]>([]);
  const [activeContent, setActiveContent] = React.useState<IContentModel | null>(null);
  const [playerOpen, setPlayerOpen] = React.useState<boolean>(false);
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

  React.useEffect(() => {
    // Need to wait until front page images are in redux store before making a request.
    if (frontPageImagesMediaTypeId) {
      const settings = filterFormat(filter, actions);
      navigate(`?${toQueryString(settings)}`);
      const query = genQuery(settings);
      fetchResults(query, filter.searchUnpublished);
    }
    // Only execute this on page load.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [frontPageImagesMediaTypeId]);

  const handleSearch = React.useCallback(async () => {
    const settings = filterFormat(filter, actions);
    navigate(`?${toQueryString(settings)}`);
    const query = genQuery(settings);
    fetchResults(query, filter.searchUnpublished);
  }, [actions, fetchResults, filter, genQuery, navigate]);

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
              <>
                <div className="title">{`Search Results`}</div>
              </>
            }
          >
            <ContentListActionBar
              content={selected}
              onSelectAll={(e) => (e.target.checked ? setSelected(content) : setSelected([]))}
              className="search"
            />
            <Show visible={!!activeFilter}>
              <div className="viewed-name padding-left">
                <FaBookmark />
                <div className="filter-name">{activeFilter?.name}</div>
              </div>
            </Show>
            <Row className="search-contents">
              <div className={playerOpen ? 'scroll minimized' : 'scroll'}>
                <Col className={'search-items'}>
                  <Show visible={!content.length}>
                    <Row className="helper-text" justifyContent="center">
                      Please refine search criteria and click "search".
                    </Row>
                  </Show>

                  {content.map((item) => {
                    return (
                      <Row key={item.id} className="rows">
                        <Col className="cols">
                          <Row>
                            <Col alignItems="center">
                              <Checkbox
                                onChange={(e) => {
                                  if (e.target.checked) {
                                    setSelected([...selected, item]);
                                  } else {
                                    setSelected(selected.filter((i) => i.id !== item.id));
                                  }
                                }}
                                className="checkbox"
                                checked={selected.some((i) => i.id === item.id)}
                              />
                            </Col>
                            <Col className="tone-date">
                              <Row>
                                <Sentiment
                                  value={item.tonePools?.length ? item.tonePools[0].value : 0}
                                />
                                <div className="date text-content">
                                  {new Date(item.publishedOn).toDateString()}
                                </div>
                                <span className="divider"> | </span>
                                <div className="source text-content">{item.source?.name}</div>
                                <Show visible={!!item.series?.name}>
                                  <span className="divider"> | </span>
                                  <div className="series text-content">{item.series?.name}</div>
                                </Show>
                              </Row>
                            </Col>
                          </Row>
                          <div
                            className="headline text-content"
                            onClick={() => navigate(`/view/${item.id}`)}
                          >
                            {formatSearch(item.headline, filter)}
                          </div>
                          {/* TODO: Extract text around keyword searched and preview that text rather than the first 50 words */}
                          <div className="summary text-content">
                            {formatSearch(determinePreview(item), filter)}
                          </div>
                          <Show visible={!!item.fileReferences?.length}>
                            <button
                              onClick={() => {
                                !playerOpen && setPlayerOpen(true);
                                item.fileReferences && setActiveContent(item);
                              }}
                              className={
                                playerOpen && activeContent?.id === item.id
                                  ? 'playing media-button'
                                  : 'show media-button'
                              }
                            >
                              {playerOpen && activeContent?.id === item.id ? (
                                <Row>
                                  <div>NOW PLAYING</div> <FaStop />
                                </Row>
                              ) : (
                                <Row>
                                  <div>PLAY MEDIA</div> <FaPlay />
                                </Row>
                              )}
                            </button>
                          </Show>
                        </Col>
                      </Row>
                    );
                  })}
                </Col>
              </div>
              <Show visible={playerOpen}>
                <Col className="player">
                  <Player setPlayerOpen={setPlayerOpen} content={activeContent} />
                </Col>
              </Show>
            </Row>
            {isLoading && <Loading />}
          </PageSection>
        </Col>
      </Row>
    </styled.SearchPage>
  );
};
