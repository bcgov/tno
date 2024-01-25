import { PageSection } from 'components/section';
import { ISubMediaGroupItem } from 'features/search-page/components/advanced-search/interfaces';
import React from 'react';
import { useContent, useLookup } from 'store/hooks';
import { Col, ISourceModel, Row, Show } from 'tno-core';

import { FilterMedia } from './FilterMedia';
import * as styled from './styled';
import { alphabetArray } from './utils';

/** The FilterMediaLanding Consists of the filter to narrow down the results of media, as well as the results. It is returned in a two column layout*/
export const FilterMediaLanding: React.FC = () => {
  const [
    {
      mediaType: { filter },
    },
    { storeMediaTypeFilter: storeFilter },
  ] = useContent();
  const [{ sources, mediaTypes }] = useLookup();
  const [activeFilter, setActiveFilter] = React.useState<ISubMediaGroupItem>();

  const [activeLetter, setActiveLetter] = React.useState<string>('All');
  const [narrowedOptions, setNarrowedOptions] = React.useState<ISourceModel[]>([]);
  const [activeSource, setActiveSource] = React.useState<ISourceModel | null>(null);
  const [subMediaGroups, setSubMediaGroups] = React.useState<ISubMediaGroupItem[]>();

  React.useEffect(() => {
    // exit early if inputs are not set completely
    if (sources.length === 0 || mediaTypes.length === 0) return;

    let mediaTypeSourceLookup: { [name: string]: ISourceModel[] } = {};
    const allSourcesKey: string = 'All';
    mediaTypeSourceLookup[allSourcesKey] = [];
    // prime the dictionary - already in sort order set on Media Type
    mediaTypes.forEach((mt) => {
      mediaTypeSourceLookup[mt.name] = [];
    });
    sources.forEach((source) => {
      mediaTypeSourceLookup[allSourcesKey].push(source);
      source.mediaTypeSearchMappings.forEach((mapping) => {
        mediaTypeSourceLookup[mapping.name].push(source);
      });
    });
    // Remove Media Type entries with no assigned Sources
    // Could also exclude specific Media Types her if required
    mediaTypeSourceLookup = Object.fromEntries(
      Object.entries(mediaTypeSourceLookup).filter(([k, v]) => v.length > 0),
    );

    let subMediaGroups: ISubMediaGroupItem[] = [];
    for (let key in mediaTypeSourceLookup) {
      // Use `key` and `value`
      let value = mediaTypeSourceLookup[key];
      subMediaGroups.push({
        key: key,
        label: key,
        options: value,
      });
    }
    setSubMediaGroups(subMediaGroups);
  }, [sources, mediaTypes, setSubMediaGroups]);

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
              {subMediaGroups?.map((mediaGroup) => (
                <div
                  key={`${mediaGroup.key}`}
                  onClick={() => {
                    if (mediaGroup.label === 'Weekly Print' || mediaGroup.label === 'All')
                      setActiveLetter('A');
                    else if (mediaGroup.label === 'Online') setActiveLetter('B');
                    else setActiveLetter('All');
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
