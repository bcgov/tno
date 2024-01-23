import { useFilterOptions } from 'components/navbar';
import { PageSection } from 'components/section';
import React from 'react';
import { Col, ISourceModel, Row } from 'tno-core';

import { MediaFilterTypes } from './constants';
import { FilterMedia } from './FilterMedia';
import * as styled from './styled';
import { alphabetArray } from './utils';

/** The FilterMediaLanding Consists of the filter to narrow down the results of media, as well as the results. It is returned in a two column layout*/
export const FilterMediaLanding: React.FC = () => {
  const {
    dailyPrint,
    sources,
    weeklyPrint,
    cpWire,
    talkRadio,
    onlinePrint,
    television,
    newsRadio,
  } = useFilterOptions();
  const [activeFilter, setActiveFilter] = React.useState<MediaFilterTypes>(
    MediaFilterTypes.DAILY_PRINT,
  );
  const [activeLetter, setActiveLetter] = React.useState<string>('All');
  const [narrowedOptions, setNarrowedOptions] = React.useState<ISourceModel[]>([]);
  const [activeSource, setActiveSource] = React.useState<ISourceModel | null>(null);

  const labels = Object.values(MediaFilterTypes);
  const determineOptions = () => {
    switch (activeFilter) {
      case MediaFilterTypes.DAILY_PRINT:
        return dailyPrint;
      case MediaFilterTypes.WEEKLY_PRINT:
        return weeklyPrint;
      case MediaFilterTypes.INTERNET:
        return onlinePrint;
      case MediaFilterTypes.TV:
        return television;
      case MediaFilterTypes.TALK_RADIO:
        return talkRadio;
      case MediaFilterTypes.RADIO_NEWS:
        return newsRadio;
      case MediaFilterTypes.CP_NEWS:
        return cpWire;
      default:
        return [];
    }
  };

  React.useEffect(() => {
    if (activeLetter && activeLetter !== 'All') {
      const options = determineOptions();
      setNarrowedOptions(options.filter((opt) => opt.name.startsWith(activeLetter)));
    }
    if (activeLetter === 'All') {
      setNarrowedOptions(determineOptions());
    }
    // only want to fire when filters change
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [activeLetter, activeFilter]);

  return (
    <styled.FilterMediaLanding>
      <Col className="filters">
        <PageSection header="Filters">
          <Row>
            <Col className="media-filter">
              {labels.map((label) => (
                <div
                  key={`${label}`}
                  onClick={() => setActiveFilter(label)}
                  className={`${activeFilter === label ? 'active' : 'inactive'} option`}
                >
                  {label}
                </div>
              ))}
            </Col>
            <Col className="narrowed-options">
              <Row className="alpha-filter">
                {alphabetArray().map((letter) => {
                  return (
                    <div
                      className={`${activeLetter === letter ? 'active' : 'inactive'}-letter`}
                      onClick={() => setActiveLetter(letter)}
                    >
                      {letter}
                    </div>
                  );
                })}
              </Row>
              {narrowedOptions.map((opt) => {
                return (
                  <div
                    onClick={() => setActiveSource(opt)}
                    className={`${
                      activeSource?.name === opt.name ? 'active' : 'inactive'
                    }-narrowed-option`}
                  >
                    {opt.name}
                  </div>
                );
              })}
            </Col>
          </Row>
          <div>test</div>
        </PageSection>
      </Col>
      <Col className="results">
        <PageSection header="Results">
          <FilterMedia />
        </PageSection>
      </Col>
    </styled.FilterMediaLanding>
  );
};
