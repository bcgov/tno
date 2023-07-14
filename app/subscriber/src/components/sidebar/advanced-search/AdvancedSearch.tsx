import { DateFilter } from 'components/date-filter';
import React from 'react';
import { BsCalendarEvent, BsSun } from 'react-icons/bs';
import { FaSearch } from 'react-icons/fa';
import { GiHamburgerMenu } from 'react-icons/gi';
import { IoIosArrowDropdownCircle, IoIosArrowDroprightCircle, IoIosCog } from 'react-icons/io';
import { Button, Col, Row, Show, Text, ToggleGroup } from 'tno-core';

import { SubMediaGroups } from './constants';
import * as styled from './styled';

export interface IAdvancedSearchProps {
  expanded: boolean;
  setExpanded: (expanded: boolean) => void;
}

/***
 * AdvancedSearch is a component that displays the advanced search form in the sidebar.
 */
export const AdvancedSearch: React.FC<IAdvancedSearchProps> = ({ expanded, setExpanded }) => {
  const [dateExpanded, setDateExpanded] = React.useState(false);
  const [mediaExpanded, setMediaExpanded] = React.useState(false);
  const searchInOptions = [
    {
      label: 'Headline',
      onClick: () => console.log('Headline'),
    },
    {
      label: 'Byline',
      onClick: () => console.log('Byline'),
    },
    {
      label: 'Story text',
      onClick: () => console.log('Story text'),
    },
  ];
  return (
    <styled.AdvancedSearch>
      <Show visible={expanded}>
        <p onClick={() => setExpanded(false)} className="back-text">
          Back to basic search
        </p>
      </Show>
      <Row className="search-bar">
        <GiHamburgerMenu />
        <Text width="11.5em" className="search-input" name="search" />
        <FaSearch />
      </Row>
      <Show visible={!expanded}>
        <p onClick={() => setExpanded(true)} className="use-text">
          Use advanced search
        </p>
      </Show>
      <Show visible={expanded}>
        <div className="search-in-group">
          <b>Search in: </b>
          <ToggleGroup className="toggles" options={searchInOptions} />
        </div>
        <Col className="section narrow">
          <b>Narrow your results by: </b>
          <Col className={`date-range-group ${dateExpanded ? 'expanded' : ''}`}>
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
            <Show visible={dateExpanded}>
              <Row className="expanded">
                <DateFilter />
              </Row>
            </Show>
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
            <Show visible={mediaExpanded}>
              <Col className="expanded">
                {SubMediaGroups.map((mediaGroup) => (
                  <Row className="sub-group">
                    {mediaGroup.label} <IoIosArrowDroprightCircle className="drop-icon" />
                  </Row>
                ))}
              </Col>
            </Show>
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
        <Button className="search-button">Search</Button>
      </Show>
    </styled.AdvancedSearch>
  );
};
