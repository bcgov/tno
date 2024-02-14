import { PageSection } from 'components/section';
import { ISubMediaGroupItem } from 'features/search-page/components/advanced-search/interfaces';
import { createSubMediaGroups } from 'features/utils/createSubMediaGroups';
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

  const [activeLetter, setActiveLetter] = React.useState<string>('A');
  const [narrowedOptions, setNarrowedOptions] = React.useState<ISourceModel[]>([]);
  const [activeSource, setActiveSource] = React.useState<ISourceModel | null>(null);
  const [subMediaGroups, setSubMediaGroups] = React.useState<ISubMediaGroupItem[]>();
  const [parentClicked, setParentClicked] = React.useState<boolean>(false);

  React.useEffect(() => {
    setActiveFilter(subMediaGroups?.find((sg) => sg.label === 'All')); // default to All
    // only want to fire when subMediaGroups loads
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [subMediaGroups]);

  /** When the parent group is clicked, we want to set the sourceIds to the sources of the parent group
   *   Additionally, we want to deselect the narrowed option
   * */
  React.useEffect(() => {
    if (parentClicked) {
      storeFilter({
        ...filter,
        sourceIds: determineSourceIds(),
      });
      setActiveSource(null); // deselect the narrowed option
      setParentClicked(false);
    }
    // only want to fire when parentClicked changes
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [parentClicked]);

  React.useEffect(() => {
    createSubMediaGroups(sources, mediaTypes, setSubMediaGroups);
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

  const determineSourceIds = () => {
    return subMediaGroups
      ?.find((sg) => sg.label === activeFilter?.label)
      ?.options.map((opt) => opt.id);
  };

  return (
    <styled.FilterMediaLanding>
      <Col className="filters">
        <PageSection ignoreLastChildGap header="Filter by Media Type" includeHeaderIcon>
          {/* TODO: Move into reusable component, this type of filter is only used on this page currently*/}
          <Row className="main-media">
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
                    setParentClicked(true);
                  }}
                  className={`${
                    activeFilter?.label === mediaGroup.label ||
                    (!activeFilter && mediaGroup.label === 'All') // default to All
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
              <div className="scroll-container">
                {narrowedOptions.map((opt) => {
                  return (
                    <div
                      key={`${opt.id}`}
                      onClick={() => {
                        setActiveSource(opt);
                        storeFilter({ ...filter, sourceIds: [opt.id] });
                      }}
                      className={`${
                        activeSource?.name === opt.name ? 'active' : 'inactive'
                      } narrowed-option`}
                    >
                      {opt.name}
                    </div>
                  );
                })}
              </div>
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
