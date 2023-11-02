import { MsearchMultisearchBody } from '@elastic/elasticsearch/lib/api/types';
import { FolderSubMenu } from 'components/folder-sub-menu';
import { SearchWithLogout } from 'components/search-with-logout';
import { Sentiment } from 'components/sentiment';
import { AdvancedSearch } from 'components/sidebar/advanced-search';
import {
  IContentListAdvancedFilter,
  IContentListFilter,
} from 'features/content/list-view/interfaces';
import { determinePreview } from 'features/utils';
import parse from 'html-react-parser';
import React from 'react';
import { FaPlay, FaSave, FaStop } from 'react-icons/fa';
import { useNavigate, useParams } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useContent, useFilters } from 'store/hooks';
import {
  Checkbox,
  Col,
  convertTo,
  fromQueryString,
  generateQuery,
  IContentModel,
  Loading,
  Row,
  Show,
  Text,
} from 'tno-core';

import { Player } from './player/Player';
import * as styled from './styled';
import { filterFormat } from './utils';

// Simple component to display users search results
export const SearchPage: React.FC = () => {
  const [, { findContentWithElasticsearch }] = useContent();
  const navigate = useNavigate();
  const [, { addFilter }] = useFilters();

  const params = useParams();
  const [searchItems, setSearchItems] = React.useState<IContentModel[]>([]);
  const [activeContent, setActiveContent] = React.useState<IContentModel | null>(null);
  const [playerOpen, setPlayerOpen] = React.useState<boolean>(false);
  const [searchName, setSearchName] = React.useState<string>('');
  const [selected, setSelected] = React.useState<IContentModel[]>([]);
  const [isLoading, setIsLoading] = React.useState(false);

  const urlParams = new URLSearchParams(params.query);

  const search = React.useMemo(
    () =>
      fromQueryString(params.query, {
        arrays: ['sourceIds', 'sentiment', 'productIds', 'actions'],
        numbers: ['sourceIds', 'sentiment', 'productIds'],
      }),
    [params.query],
  );

  const advancedSubscriberFilter: IContentListFilter & Partial<IContentListAdvancedFilter> =
    React.useMemo(() => {
      return {
        useUnpublished: urlParams.get('useUnpublished') === 'true' ?? false,
        keyword: urlParams.get('keyword') ?? '',
        searchTerm: urlParams.get('searchTerm') ?? '',
        inByline: urlParams.get('inByline') === 'true' ?? false,
        inHeadline: urlParams.get('inHeadline') === 'true' ?? false,
        inStory: urlParams.get('inStory') === 'true' ?? false,
        contentTypes: [],
        actions: search.actions?.map((v: any) => convertTo(v, 'string', undefined)),
        hasFile: urlParams.get('hasFile') === 'true' ?? false,
        headline: urlParams.get('headline') ?? '',
        pageIndex: convertTo(urlParams.get('pageIndex'), 'number', 0),
        pageSize: convertTo(urlParams.get('pageSize'), 'number', 100),
        sourceIds: search.sourceIds?.map((v: any) => convertTo(v, 'number', undefined)),
        productIds: search.productIds?.map((v: any) => convertTo(v, 'number', undefined)),
        sentiment: search.sentiment?.map((v: any) => convertTo(v, 'number', undefined)),
        startDate: urlParams.get('publishedStartOn') ?? '',
        endDate: urlParams.get('publishedEndOn') ?? '',
        storyText: urlParams.get('storyText') ?? '',
        boldKeywords: urlParams.get('boldKeywords') === 'true' ?? '',
        topStory: urlParams.get('actions') === 'Top Story' ?? '',
        sort: [],
      };
      // only want this to update when the query changes
      // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [params.query]);

  // function that bolds the searched text only if advanced filter is enabled for it
  const formatSearch = React.useCallback(
    (text: string) => {
      let tempText = text;
      let parseText = () => {
        if (advancedSubscriberFilter.searchTerm) return advancedSubscriberFilter.searchTerm;
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
      if (!advancedSubscriberFilter.boldKeywords) return parse(text);
      return parse(tempText);
    },
    [advancedSubscriberFilter.searchTerm, advancedSubscriberFilter.boldKeywords],
  );

  const fetchResults = React.useCallback(
    async (filter: MsearchMultisearchBody) => {
      try {
        setIsLoading(true);
        const res: any = await findContentWithElasticsearch(filter, false);
        setSearchItems(res.hits.hits.map((h: { _source: IContentModel }) => h._source));
      } catch {
      } finally {
        setIsLoading(false);
      }
    },
    [findContentWithElasticsearch],
  );

  const saveSearch = React.useCallback(async () => {
    const data = await addFilter({
      name: searchName,
      query: generateQuery(filterFormat(advancedSubscriberFilter)),
      settings: { ...filterFormat(advancedSubscriberFilter) },
      id: 0,
      sortOrder: 0,
      description: '',
      isEnabled: true,
      reports: [],
      folders: [],
    });
    toast.success(`${data.name} has successfully been saved.`);
  }, [advancedSubscriberFilter, searchName, addFilter]);

  /** retrigger content fetch when change is applied */
  React.useEffect(() => {
    fetchResults(generateQuery(filterFormat(advancedSubscriberFilter)));
    // only run when query changes
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [params.query]);

  return (
    <styled.SearchPage>
      <SearchWithLogout />
      <Row className="search-container">
        <Col className="adv-search-container">
          <AdvancedSearch onSearchPage expanded={true} />
        </Col>
        <Col className="result-container">
          <Row className="save-bar">
            <div className="label">Name this search: </div>
            <Text
              onChange={(e) => {
                setSearchName(e.target.value);
              }}
              name="searchName"
            />
            <FaSave className="save-button" onClick={() => saveSearch()} />
            <FolderSubMenu selectedContent={selected} />
          </Row>
          <Row>
            <div className={playerOpen ? 'scroll minimized' : 'scroll'}>
              <Col className={'search-items'}>
                {searchItems.map((item) => {
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
        </Col>
      </Row>
    </styled.SearchPage>
  );
};
