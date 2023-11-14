import { makeFilter } from 'features';
import { useParamsToFilter } from 'features/search-page/hooks';
import { filterFormat } from 'features/search-page/utils';
import React from 'react';
import { BsCalendarEvent, BsSun } from 'react-icons/bs';
import { FaPlay, FaRegSmile, FaSearch } from 'react-icons/fa';
import { FaCloudArrowUp } from 'react-icons/fa6';
import {
  IoIosArrowDropdownCircle,
  IoIosArrowDroprightCircle,
  IoIosCog,
  IoMdRefresh,
} from 'react-icons/io';
import { useNavigate } from 'react-router';
import { useParams } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useFilters, useLookup } from 'store/hooks';
import { Button, Col, generateQuery, Row, Show, Text, TextArea, toQueryString } from 'tno-core';

import {
  DateSection,
  MediaSection,
  MoreOptions,
  SearchInGroup,
  SentimentSection,
} from './components';
import { defaultAdvancedSearch } from './constants';
import {
  defaultSubMediaGroupExpanded,
  IAdvancedSearchFilter,
  ISubMediaGroupExpanded,
} from './interfaces';
import * as styled from './styled';
import { queryToState } from './utils/queryToState';

export interface IAdvancedSearchProps {
  expanded: boolean;
  setExpanded?: (expanded: boolean) => void;
  onSearchPage?: boolean;
}

/***
 * AdvancedSearch is a component that displays the advanced search form in the sidebar.
 * @param expanded - determines whether the advanced search form is expanded or not
 * @param setExpanded - function that controls the expanded state of the advanced search form
 */
export const AdvancedSearch: React.FC<IAdvancedSearchProps> = ({
  expanded,
  setExpanded,
  onSearchPage,
}) => {
  const navigate = useNavigate();
  const [, { addFilter }] = useFilters();
  const [{ actions }] = useLookup();
  const { advancedSubscriberFilter } = useParamsToFilter();
  const { query } = useParams();
  /** determines whether the date filter section is shown or not */
  const [dateExpanded, setDateExpanded] = React.useState(false);
  /** determines whether the additional options are expanded  */
  const [optionsExpanded, setOptionsExpanded] = React.useState(false);
  /** determines whether the search in section is shown or not */
  const [searchExpanded] = React.useState(false);
  /** controls the parent group "Media Sources" expansion */
  const [mediaExpanded, setMediaExpanded] = React.useState(false);
  /** controls the sub group states for media sources. i.e) whether Daily Papers is expanded */
  const [mediaGroupExpandedStates, setMediaGroupExpandedStates] =
    React.useState<ISubMediaGroupExpanded>(defaultSubMediaGroupExpanded);
  /** controls the sub group state for sentiment */
  const [sentimentExpanded, setSentimentExpanded] = React.useState(false);
  const [searchName, setSearchName] = React.useState<string>('');
  /** the object that will eventually be converted to a query and be passed to elastic search */
  const [advancedSearch, setAdvancedSearch] =
    React.useState<IAdvancedSearchFilter>(defaultAdvancedSearch);
  const [constants, setConstants] = React.useState<any>({});

  // update state when query changes, necessary to keep state in sync with url when navigating directly
  React.useEffect(() => {
    if (query) setAdvancedSearch(queryToState(query.toString()));
  }, [query]);

  React.useEffect(() => {
    fetch('/constants.json')
      .then((res) => res.json())
      .then((data) => {
        setConstants(data);
      });
  }, []);

  const advancedFilter = React.useMemo(
    () =>
      makeFilter({
        actions: advancedSearch?.topStory ? ['Top Story'] : [],
        boldKeywords: advancedSearch?.boldKeywords,
        inByline: advancedSearch?.inByline,
        inHeadline: advancedSearch?.inHeadline,
        inStory: advancedSearch?.inStory,
        searchTerm: advancedSearch?.searchTerm,
        mediaTypeIds: advancedSearch?.frontPage ? [constants?.frontPageId] : [],
        startDate: advancedSearch?.startDate,
        sourceIds: advancedSearch?.sourceIds,
        sentiment: advancedSearch?.sentiment,
        endDate: advancedSearch?.endDate,
        topStory: advancedSearch?.topStory,
        contentTypes: [],
        sort: [],
        pageIndex: 0,
        pageSize: 100,
        useUnpublished: advancedSearch?.useUnpublished,
      }),
    [advancedSearch, constants?.frontPageId],
  );

  const saveSearch = React.useCallback(async () => {
    await addFilter({
      name: searchName,
      query: generateQuery(filterFormat(advancedSubscriberFilter, actions)),
      settings: { ...filterFormat(advancedSubscriberFilter, actions) },
      id: 0,
      sortOrder: 0,
      description: '',
      isEnabled: true,
      reports: [],
      folders: [],
    })
      .then((data) => toast.success(`${data.name} has successfully been saved.`))
      .catch();
  }, [advancedSubscriberFilter, searchName, addFilter, actions]);

  const handleSearch = async () => {
    navigate(`/search/${toQueryString(advancedFilter, { includeEmpty: false })}`);
  };

  /** allow user to hit enter while searching */
  const enterPressed = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter') {
      handleSearch();
    }
  };

  return (
    <styled.AdvancedSearch expanded={expanded}>
      <div className="main-search-body">
        <Show visible={expanded}>
          <Row className="top-bar">
            <div className="title">Advanced Search</div>
            <Show visible={!onSearchPage}>
              <p onClick={() => !!setExpanded && setExpanded(false)} className="back-text">
                BACK TO BASIC
              </p>
            </Show>
            <IoMdRefresh
              className="reset"
              data-tooltip-id="main-tooltip"
              data-tooltip-content="Reset filters"
              onClick={() => {
                setAdvancedSearch(defaultAdvancedSearch);
              }}
            />
          </Row>
        </Show>
        <Row className="search-for-row">
          <label className={expanded ? 'label-expanded' : 'label'}>SEARCH FOR: </label>
          <Show visible={!expanded}>
            <Row className="search-bar space-top">
              <FaSearch onClick={() => handleSearch()} className="search-icon" />
              <Text
                className="search-input"
                onKeyDown={enterPressed}
                value={advancedSearch?.searchTerm}
                name="search"
                onChange={(e) =>
                  setAdvancedSearch({ ...advancedSearch, searchTerm: e.target.value })
                }
              />
            </Row>
          </Show>
          <Show visible={expanded}>
            <Col>
              <TextArea
                value={advancedSearch?.searchTerm}
                className="text-area"
                onKeyDown={enterPressed}
                name="search"
                onChange={(e) =>
                  setAdvancedSearch({ ...advancedSearch, searchTerm: e.target.value })
                }
              />
              <SearchInGroup
                advancedSearch={advancedSearch}
                searchExpanded={searchExpanded}
                setAdvancedSearch={setAdvancedSearch}
              />
            </Col>
          </Show>
          <Show visible={!expanded}>
            <Button
              className="search-button"
              onClick={() => {
                handleSearch();
              }}
            >
              Search <FaPlay />
            </Button>
          </Show>
          <Show visible={!expanded}>
            <p onClick={() => handleSearch()} className="use-text">
              GO ADVANCED
            </p>
          </Show>
        </Row>

        <Show visible={expanded}>
          <div className="search-in-group space-top"></div>
          <Col className="section narrow">
            <b>Narrow your results by: </b>
            <Col className={`date-range-group space-top ${dateExpanded ? 'expanded' : ''}`}>
              <Row className="option-row" onClick={() => setDateExpanded(!dateExpanded)}>
                <BsCalendarEvent /> Date range
              </Row>
              <DateSection advancedSearch={advancedSearch} setAdvancedSearch={setAdvancedSearch} />
            </Col>
            <Col className={`media-group ${mediaExpanded ? 'expanded' : ''}`}>
              <Row className="option-row" onClick={() => setMediaExpanded(!mediaExpanded)}>
                <BsSun />
                Media source
                {!mediaExpanded ? (
                  <IoIosArrowDroprightCircle
                    onClick={() => setMediaExpanded(true)}
                    className="drop-icon"
                  />
                ) : (
                  <IoIosArrowDropdownCircle
                    onClick={() => setMediaExpanded(false)}
                    className="drop-icon"
                  />
                )}
              </Row>
              <MediaSection
                advancedSearch={advancedSearch}
                setAdvancedSearch={setAdvancedSearch}
                mediaExpanded={mediaExpanded}
                setmediaGroupExpandedStates={setMediaGroupExpandedStates}
                mediaGroupExpandedStates={mediaGroupExpandedStates}
              />
            </Col>
            <Col className={`sentiment-group ${sentimentExpanded ? 'expanded' : ''}`}>
              <Row className="option-row" onClick={() => setSentimentExpanded(!sentimentExpanded)}>
                <FaRegSmile />
                Sentiment
                {!sentimentExpanded ? (
                  <IoIosArrowDroprightCircle
                    onClick={() => setSentimentExpanded(true)}
                    className="drop-icon"
                  />
                ) : (
                  <IoIosArrowDropdownCircle
                    onClick={() => setSentimentExpanded(false)}
                    className="drop-icon"
                  />
                )}
              </Row>
              <SentimentSection
                sentimentExpanded={sentimentExpanded}
                advancedSearch={advancedSearch}
                setAdvancedSearch={setAdvancedSearch}
              />
            </Col>
          </Col>
          <Row className="section">
            <Col className="section">
              <b>Display options:</b>
              <Row className="search-options-group option-row">
                <div className="initial-row" onClick={() => setOptionsExpanded(!optionsExpanded)}>
                  <IoIosCog />
                  Search result options
                  {!optionsExpanded ? (
                    <IoIosArrowDroprightCircle
                      className="drop-icon"
                      onClick={() => setOptionsExpanded(true)}
                    />
                  ) : (
                    <IoIosArrowDropdownCircle
                      onClick={() => setOptionsExpanded(false)}
                      className="drop-icon"
                    />
                  )}
                </div>
                <MoreOptions
                  advancedSearch={advancedSearch}
                  setAdvancedSearch={setAdvancedSearch}
                  optionsExpanded={optionsExpanded}
                />
              </Row>
            </Col>
          </Row>
        </Show>
      </div>
      <Show visible={expanded}>
        <Row className="adv-toolbar">
          <div className="label">SAVE SEARCH AS: </div>
          <Text
            onChange={(e) => {
              setSearchName(e.target.value);
            }}
            name="searchName"
          />
          <button className="save-cloud">
            <FaCloudArrowUp onClick={() => saveSearch()} />
          </button>
          <Button
            onClick={() => {
              handleSearch();
            }}
            className="search-button-expanded"
          >
            Search
            <FaPlay />
          </Button>
        </Row>
      </Show>
    </styled.AdvancedSearch>
  );
};
