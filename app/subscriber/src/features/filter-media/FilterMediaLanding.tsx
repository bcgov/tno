import { useFilterOptions } from 'components/navbar';
import { PageSection } from 'components/section';
import React from 'react';
import { useContent, useLookup } from 'store/hooks';
import { Col, ISourceModel, Row, Show } from 'tno-core';

import { MediaFilterTypes } from './constants';
import { FilterMedia } from './FilterMedia';
import * as styled from './styled';
import { alphabetArray } from './utils';

/** The FilterMediaLanding Consists of the filter to narrow down the results of media, as well as the results. It is returned in a two column layout*/
export const FilterMediaLanding: React.FC = () => {
  const { dailyPrint, weeklyPrint, cpWire, talkRadio, onlinePrint, television, newsRadio } =
    useFilterOptions();
  const [{ mediaTypes }] = useLookup();
  console.log(mediaTypes);
  const [
    {
      mediaType: { filter },
    },
    { storeMediaTypeFilter: storeFilter },
  ] = useContent();
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

  const determineShowAll = () => {
    switch (activeFilter) {
      case MediaFilterTypes.DAILY_PRINT:
        storeFilter({ ...filter, sourceIds: dailyPrint.map((opt) => opt.id) });
        break;
      case MediaFilterTypes.WEEKLY_PRINT:
        storeFilter({ ...filter, sourceIds: weeklyPrint.map((opt) => opt.id) });
        break;
      case MediaFilterTypes.INTERNET:
        storeFilter({ ...filter, sourceIds: onlinePrint.map((opt) => opt.id) });
        break;
      case MediaFilterTypes.TV:
        storeFilter({ ...filter, sourceIds: television.map((opt) => opt.id) });
        break;
      case MediaFilterTypes.TALK_RADIO:
        storeFilter({ ...filter, sourceIds: talkRadio.map((opt) => opt.id) });
        break;
      case MediaFilterTypes.RADIO_NEWS:
        storeFilter({ ...filter, sourceIds: newsRadio.map((opt) => opt.id) });
        break;
      case MediaFilterTypes.CP_NEWS:
        storeFilter({ ...filter, sourceIds: cpWire.map((opt) => opt.id) });
        break;
      default:
        break;
    }
  };

  React.useEffect(() => {
    if (dailyPrint && !narrowedOptions.length) setNarrowedOptions(dailyPrint);
  }, [dailyPrint, narrowedOptions.length]);

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

  /** When the active filter changes, we want to show all options in that category before the user narrows it down*/
  React.useEffect(() => {
    determineShowAll();
    // only want to fire when filters change
    //eslint-disable-next-line react-hooks/exhaustive-deps
  }, [activeFilter]);

  return (
    <styled.FilterMediaLanding>
      <Col className="filters">
        <PageSection ignoreLastChildGap header="Filter by Media Type">
          {/* TODO: Move into reusable component, this type of filter is only used on this page currently*/}
          <Row>
            <Col className="media-filter">
              {labels.map((label) => (
                <div
                  key={`${label}`}
                  onClick={() => {
                    if (label === MediaFilterTypes.WEEKLY_PRINT) setActiveLetter('A');
                    if (label === MediaFilterTypes.INTERNET) setActiveLetter('B');
                    setActiveFilter(label);
                  }}
                  className={`${activeFilter === label ? 'active' : 'inactive'} option`}
                >
                  {label}
                </div>
              ))}
            </Col>
            <Col className="narrowed-options">
              <Show
                visible={
                  activeFilter === MediaFilterTypes.INTERNET ||
                  activeFilter === MediaFilterTypes.WEEKLY_PRINT
                }
              >
                <Row className="alpha-filter">
                  {alphabetArray().map((letter) => {
                    return (
                      <div
                        className={`${activeLetter === letter ? 'active' : 'inactive'}-letter`}
                        onClick={() => setActiveLetter(letter)}
                        key={`${letter}`}
                      >
                        {letter}
                      </div>
                    );
                  })}
                </Row>
              </Show>
              <div
                onClick={() => {
                  setActiveSource(null);
                  determineShowAll();
                }}
                className="show-all"
              >
                Show all
              </div>
              {narrowedOptions.map((opt) => {
                return (
                  <div
                    onClick={() => {
                      setActiveSource(opt);
                      storeFilter({ ...filter, sourceIds: [opt.id] });
                    }}
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
        </PageSection>
      </Col>
      <Col className="results">
        <PageSection>
          <FilterMedia />
        </PageSection>
      </Col>
    </styled.FilterMediaLanding>
  );
};
