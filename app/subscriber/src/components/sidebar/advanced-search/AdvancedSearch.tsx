import { makeFilter } from 'features';
import React from 'react';
import { BsCalendarEvent, BsSun } from 'react-icons/bs';
import { FaRegSmile, FaSearch } from 'react-icons/fa';
import { GiHamburgerMenu } from 'react-icons/gi';
import {
  IoIosArrowDropdownCircle,
  IoIosArrowDroprightCircle,
  IoIosCog,
  IoMdRefresh,
} from 'react-icons/io';
import { useNavigate } from 'react-router';
import { useParams } from 'react-router-dom';
import { Button, Col, Row, Show, Text, toQueryString } from 'tno-core';

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
  setExpanded: (expanded: boolean) => void;
}

/***
 * AdvancedSearch is a component that displays the advanced search form in the sidebar.
 * @param expanded - determines whether the advanced search form is expanded or not
 * @param setExpanded - function that controls the expanded state of the advanced search form
 */
export const AdvancedSearch: React.FC<IAdvancedSearchProps> = ({ expanded, setExpanded }) => {
  const navigate = useNavigate();
  const { query } = useParams();
  /** determines whether the date filter section is shown or not */
  const [dateExpanded, setDateExpanded] = React.useState(false);
  /** determines whether the additional options are expanded  */
  const [optionsExpanded, setOptionsExpanded] = React.useState(false);
  /** determines whether the search in section is shown or not */
  const [searchExpanded, setSearchExpanded] = React.useState(false);
  /** controls the parent group "Media Sources" expansion */
  const [mediaExpanded, setMediaExpanded] = React.useState(false);
  /** controls the sub group states for media sources. i.e) whether Daily Papers is expanded */
  const [mediaGroupExpandedStates, setMediaGroupExpandedStates] =
    React.useState<ISubMediaGroupExpanded>(defaultSubMediaGroupExpanded);
  /** controls the sub group state for sentiment */
  const [sentimentExpanded, setSentimentExpanded] = React.useState(false);
  /** the object that will eventually be converted to a query and be passed to elastic search */
  const [advancedSearch, setAdvancedSearch] =
    React.useState<IAdvancedSearchFilter>(defaultAdvancedSearch);

  // update state when query changes, necessary to keep state in sync with url when navigating directly
  React.useEffect(() => {
    if (query) setAdvancedSearch(queryToState(query.toString()));
  }, [query]);

  const advancedFilter = React.useMemo(
    () =>
      makeFilter({
        headline:
          advancedSearch?.searchTerm && advancedSearch.searchInField?.headline
            ? advancedSearch.searchTerm
            : '',
        byline:
          advancedSearch?.searchTerm && advancedSearch.searchInField?.byline
            ? advancedSearch.searchTerm
            : '',
        hasFile: advancedSearch?.hasFile,
        storyText:
          advancedSearch?.searchTerm && advancedSearch.searchInField?.storyText
            ? advancedSearch.searchTerm
            : '',
        keyword:
          advancedSearch?.searchTerm &&
          advancedSearch.searchInField &&
          Object.values(advancedSearch.searchInField).every((v) => v === false)
            ? advancedSearch.searchTerm
            : '',
        startDate: advancedSearch?.startDate,
        sourceIds: advancedSearch?.frontPage ? [11] : advancedSearch?.sourceIds,
        sentiment: advancedSearch?.sentiment,
        endDate: advancedSearch?.endDate,
        topStory: advancedSearch?.topStory,
        contentTypes: [],
        sort: [],
        pageIndex: 0,
        pageSize: 100,
      }),
    [advancedSearch],
  );

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
    <styled.AdvancedSearch>
      <Show visible={expanded}>
        <Row className="top-bar">
          <p onClick={() => setExpanded(false)} className="back-text">
            {`<< Back to basic search`}
          </p>
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
      <Row className="search-bar space-top">
        <GiHamburgerMenu />
        <Text
          width="10.5em"
          className="search-input"
          placeholder="Search for..."
          onKeyDown={enterPressed}
          value={advancedSearch?.searchTerm}
          name="search"
          onChange={(e) => setAdvancedSearch({ ...advancedSearch, searchTerm: e.target.value })}
        />
        <FaSearch onClick={() => handleSearch()} className="search-icon" />
      </Row>
      <Show visible={!expanded}>
        <p onClick={() => setExpanded(true)} className="use-text">
          Use advanced search
        </p>
      </Show>
      <Show visible={expanded}>
        <div className="search-in-group space-top">
          {/* <b>Search in: </b>
          <ToggleGroup
            defaultSelected={
              !advancedSearch?.searchInField
                ? toggleOptions.keyword
                : toggleOptions[advancedSearch?.searchInField as keyof typeof toggleOptions]
            }
            className="toggles"
            options={searchInOptions}
          /> */}
        </div>
        <Col className="section narrow">
          <b>Narrow your results by: </b>
          <Col className={`date-range-group space-top ${dateExpanded ? 'expanded' : ''}`}>
            <Row>
              <BsCalendarEvent /> Date range
              {!dateExpanded ? (
                <IoIosArrowDroprightCircle
                  onClick={() => setDateExpanded(true)}
                  className="drop-icon"
                />
              ) : (
                <IoIosArrowDropdownCircle
                  onClick={() => setDateExpanded(false)}
                  className="drop-icon"
                />
              )}
            </Row>
            <DateSection
              advancedSearch={advancedSearch}
              dateExpanded={dateExpanded}
              setAdvancedSearch={setAdvancedSearch}
            />
          </Col>
          <Col className={`media-group ${mediaExpanded ? 'expanded' : ''}`}>
            <Row>
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
            <Row>
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
          <Col className={`search-in-group ${searchExpanded ? 'expanded' : ''}`}>
            <Row>
              <FaSearch />
              Search in
              {!searchExpanded ? (
                <IoIosArrowDroprightCircle
                  onClick={() => setSearchExpanded(true)}
                  className="drop-icon"
                />
              ) : (
                <IoIosArrowDropdownCircle
                  onClick={() => setSearchExpanded(false)}
                  className="drop-icon"
                />
              )}
            </Row>
            <SearchInGroup
              advancedSearch={advancedSearch}
              searchExpanded={searchExpanded}
              setAdvancedSearch={setAdvancedSearch}
            />
          </Col>
        </Col>
        <Row className="section">
          <Col className="section">
            <b>Display options:</b>
            <Row className="search-options-group">
              <IoIosCog />
              Search result options
              <IoIosArrowDroprightCircle
                className="drop-icon"
                onClick={() => setOptionsExpanded(true)}
              />
              <MoreOptions
                advancedSearch={advancedSearch}
                setAdvancedSearch={setAdvancedSearch}
                optionsExpanded={optionsExpanded}
              />
            </Row>
            <Row className="story-options-group">
              <GiHamburgerMenu />
              Story content options
              <IoIosArrowDroprightCircle className="drop-icon" />
            </Row>
          </Col>
        </Row>
      </Show>
      <Show visible={expanded}>
        <Button
          onClick={() => {
            handleSearch();
          }}
          className="search-button"
        >
          Search
        </Button>
      </Show>
    </styled.AdvancedSearch>
  );
};
