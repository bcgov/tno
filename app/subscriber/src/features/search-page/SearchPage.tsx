import { MsearchMultisearchBody } from '@elastic/elasticsearch/lib/api/types';
import { BasicSearch } from 'components/basic-search';
import { PageSection } from 'components/section';
import { Sentiment } from 'components/sentiment';
import { AdvancedSearch } from 'components/sidebar/advanced-search';
import { ContentListActionBar } from 'components/tool-bar';
import { useElastic } from 'features/my-searches/hooks';
import { determinePreview } from 'features/utils';
import parse from 'html-react-parser';
import React from 'react';
import { FaPlay, FaStop } from 'react-icons/fa';
import { FaBookmark } from 'react-icons/fa6';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useContent, useLookup } from 'store/hooks';
import { Checkbox, Col, IContentModel, Loading, Row, Show } from 'tno-core';

import { MySearchesSection } from './MySearchesSection';
import { Player } from './player/Player';
import * as styled from './styled';
import { filterFormat } from './utils';

export interface ISearchType {
  showAdvanced?: boolean;
}

// Simple component to display users search results
export const SearchPage: React.FC<ISearchType> = ({ showAdvanced }) => {
  const [
    {
      search: { filter },
    },
    { findContentWithElasticsearch },
  ] = useContent();
  const navigate = useNavigate();
  const [{ actions }] = useLookup();
  const [searchParams] = useSearchParams();
  const genQuery = useElastic();

  const [content, setContent] = React.useState<IContentModel[]>([]);
  const [activeContent, setActiveContent] = React.useState<IContentModel | null>(null);
  const [playerOpen, setPlayerOpen] = React.useState<boolean>(false);
  const [selected, setSelected] = React.useState<IContentModel[]>([]);
  const [isLoading, setIsLoading] = React.useState(false);

  const searchName = React.useMemo(() => searchParams.get('name'), [searchParams]);
  const viewing = React.useMemo(() => searchParams.get('viewing'), [searchParams]);

  // function that bolds the searched text only if advanced filter is enabled for it
  const formatSearch = React.useCallback(
    (text: string) => {
      let tempText = text;
      let parseText = () => {
        if (filter.search) return filter.search;
        else return '';
      };
      parseText()
        .split(' e')
        .forEach((word) => {
          const regex = new RegExp(word ?? '', 'gi');
          // remove duplicates found only want unique matches, this will be varying capitalization
          const matches = text.match(regex)?.filter((v, i, a) => a.indexOf(v) === i) ?? [];
          // text.match included in replace in order to keep the proper capitalization
          // When there is more than one match, this indicates there will be varying capitalization. In this case we
          // have to iterate through the matches and do a more specific replace in order to keep the words capitalization
          if (matches.length > 1) {
            matches.forEach((match, i) => {
              let multiMatch = new RegExp(`${matches[i]}`);
              tempText = tempText.replace(multiMatch, `<b>${match}</b>`);
            });
          } else {
            // in this case there will only be one match, so we can just insert the first match
            tempText = tempText.replace(regex, `<b>${matches[0]}</b>`);
          }
        });
      if (!filter.boldKeywords) return parse(text);
      return parse(tempText);
    },
    [filter.search, filter.boldKeywords],
  );

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

  /** retrigger content fetch when change is applied */
  React.useEffect(() => {
    const query = genQuery(filterFormat(filter, actions));
    !!window.location.search && fetchResults(query, filter.searchUnpublished);
    // only run when query  and is present
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [window.location.search]);

  return (
    <styled.SearchPage>
      <Row className="search-container">
        {/* LEFT SIDE */}
        <Show visible={showAdvanced}>
          <Col className="adv-search-container">
            {!!viewing ? <MySearchesSection /> : <AdvancedSearch onSearchPage />}
          </Col>
        </Show>
        {/* RIGHT SIDE */}
        <Col className={showAdvanced ? 'result-container' : 'result-container-full'}>
          <Show visible={!showAdvanced}>
            <BasicSearch />
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
            <Show visible={!!searchName || !!viewing}>
              <div className="viewed-name padding-left">
                <FaBookmark />
                <div className="filter-name">{searchName ?? viewing}</div>
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
                            {formatSearch(item.headline)}
                          </div>
                          {/* TODO: Extract text around keyword searched and preview that text rather than the first 50 words */}
                          <div className="summary text-content">
                            {formatSearch(determinePreview(item))}
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
