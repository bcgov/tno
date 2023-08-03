import { makeFilter } from 'features';
import React from 'react';
import { BsCalendarEvent, BsSun } from 'react-icons/bs';
import { FaRegSmile, FaSearch } from 'react-icons/fa';
import { GiHamburgerMenu } from 'react-icons/gi';
import { IoIosArrowDropdownCircle, IoIosArrowDroprightCircle, IoIosCog } from 'react-icons/io';
import { useNavigate } from 'react-router';
import { Button, Col, ContentStatus, Row, Show, Text, ToggleGroup, toQueryString } from 'tno-core';

import { DateSection, MediaSection, SentimentSection } from './components';
import { SearchInFieldName } from './constants';
import {
  defaultSubMediaGroupExpanded,
  IAdvancedSearchFilter,
  ISubMediaGroupExpanded,
} from './interfaces';
import * as styled from './styled';

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

  /** determines whether the date filter section is shown or not */
  const [dateExpanded, setDateExpanded] = React.useState(false);
  /** controls the parent group "Media Sources" expansion */
  const [mediaExpanded, setMediaExpanded] = React.useState(false);
  /** controls the sub group states for media sources. i.e) whether Daily Papers is expanded */
  const [mediaGroupExpandedStates, setMediaGroupExpandedStates] =
    React.useState<ISubMediaGroupExpanded>(defaultSubMediaGroupExpanded);
  /** controls the sub group statee for sentiment */
  const [sentimentExpanded, setSentimentExpanded] = React.useState(false);
  /** the object that will eventually be converted to a query and be passed to elastic search */
  const [advancedSearch, setAdvancedSearch] = React.useState<IAdvancedSearchFilter>({
    searchTerm: '',
    searchInField: '',
    startDate: '',
    endDate: '',
  });

  const advancedFilter = React.useMemo(
    () =>
      makeFilter({
        headline:
          advancedSearch?.searchTerm && advancedSearch.searchInField === SearchInFieldName.Headline
            ? advancedSearch.searchTerm
            : '',
        byline:
          advancedSearch?.searchTerm && advancedSearch.searchInField === SearchInFieldName.Byline
            ? advancedSearch.searchTerm
            : '',
        storyText:
          advancedSearch?.searchTerm && advancedSearch.searchInField === SearchInFieldName.StoryText
            ? advancedSearch.searchTerm
            : '',
        keyword:
          advancedSearch?.searchTerm && !advancedSearch.searchInField
            ? advancedSearch.searchTerm
            : '',
        startDate: advancedSearch?.startDate,
        sourceIds: advancedSearch?.sourceIds,
        sentiment: advancedSearch?.sentiment,
        endDate: advancedSearch?.endDate,
        status: ContentStatus.Published,
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

  const searchInOptions = [
    {
      label: 'Headline',
      onClick: () => setAdvancedSearch({ ...advancedSearch, searchInField: 'headline' }),
    },
    {
      label: 'Byline',
      onClick: () => setAdvancedSearch({ ...advancedSearch, searchInField: 'byline' }),
    },
    {
      label: 'Story text',
      onClick: () => setAdvancedSearch({ ...advancedSearch, searchInField: 'storyText' }),
    },
  ];

  /** allow user to hit enter while searching */
  const enterPressed = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter') {
      handleSearch();
    }
  };

  return (
    <styled.AdvancedSearch>
      <Show visible={expanded}>
        <p onClick={() => setExpanded(false)} className="back-text">
          {`<< Back to basic search`}
        </p>
      </Show>
      <Row className="search-bar">
        <GiHamburgerMenu />
        <Text
          width="10.5em"
          className="search-input"
          placeholder="Search for..."
          onKeyDown={enterPressed}
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
          <b>Search in: </b>
          <ToggleGroup className="toggles" options={searchInOptions} />
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
          {/*  */}
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
        </Col>
        <Row className="section">
          <Col className="section">
            <b>Display options:</b>
            <Row className="search-options-group">
              <IoIosCog />
              Search result options
              <IoIosArrowDroprightCircle className="drop-icon" />
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
        <Button onClick={() => handleSearch()} className="search-button">
          Search
        </Button>
      </Show>
    </styled.AdvancedSearch>
  );
};
