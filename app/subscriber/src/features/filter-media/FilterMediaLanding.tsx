import { useFilterOptions } from 'components/navbar';
import { PageSection } from 'components/section';
import {
  ISubMediaGroupItem,
  SubMediaGroups,
} from 'features/search-page/components/advanced-search/constants';
import React from 'react';
import { useContent } from 'store/hooks';
import { Col, ISourceModel, Row, Show } from 'tno-core';

import { FilterMedia } from './FilterMedia';
import * as styled from './styled';
import { alphabetArray } from './utils';

/** The FilterMediaLanding Consists of the filter to narrow down the results of media, as well as the results. It is returned in a two column layout*/
export const FilterMediaLanding: React.FC = () => {
  const {
    dailyPrint,
    weeklyPrint,
    cpWire,
    talkRadio,
    onlinePrint,
    television,
    newsRadio,
    sources,
  } = useFilterOptions();
  const [
    {
      mediaType: { filter },
    },
    { storeMediaTypeFilter: storeFilter },
  ] = useContent();
  const [activeFilter, setActiveFilter] = React.useState<ISubMediaGroupItem>();

  const [activeLetter, setActiveLetter] = React.useState<string>('All');
  const [narrowedOptions, setNarrowedOptions] = React.useState<ISourceModel[]>([]);
  const [activeSource, setActiveSource] = React.useState<ISourceModel | null>(null);

  React.useEffect(() => {
    if (dailyPrint && !narrowedOptions.length) setNarrowedOptions(dailyPrint);
  }, [dailyPrint, narrowedOptions.length]);

  React.useEffect(() => {
    if (activeLetter && activeLetter !== 'All') {
      setNarrowedOptions(
        activeFilter?.options.filter((opt) => opt.name.startsWith(activeLetter)) ?? [],
      );
    }
    if (activeLetter === 'All') {
      setNarrowedOptions(activeFilter?.options ?? []);
    }
    // only want to fire when filters change
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [activeLetter, activeFilter]);

  return (
    <styled.FilterMediaLanding>
      <Col className="filters">
        <PageSection ignoreLastChildGap header="Filter by Media Type">
          {/* TODO: Move into reusable component, this type of filter is only used on this page currently*/}
          <Row>
            <Col className="media-filter">
              {SubMediaGroups(
                dailyPrint,
                weeklyPrint,
                cpWire,
                talkRadio,
                onlinePrint,
                newsRadio,
                television,
                sources,
              ).map((mediaGroup) => (
                <div
                  key={`${mediaGroup.key}`}
                  onClick={() => {
                    if (mediaGroup.label === 'Weekly Print' || mediaGroup.label === 'All')
                      setActiveLetter('A');
                    if (mediaGroup.label === 'Online') setActiveLetter('B');
                    setActiveFilter(mediaGroup);
                  }}
                  className={`${
                    activeFilter?.label === mediaGroup.label ||
                    (!activeFilter && mediaGroup.label === 'Daily Print')
                      ? 'active'
                      : 'inactive'
                  } option`}
                >
                  {mediaGroup.label}
                </div>
              ))}
            </Col>
            <Col className="narrowed-options">
              <Show
                visible={
                  activeFilter?.label === 'Online' ||
                  activeFilter?.label === 'Weekly Print' ||
                  activeFilter?.label === 'All'
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
                  setNarrowedOptions(activeFilter?.options ?? []);
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
